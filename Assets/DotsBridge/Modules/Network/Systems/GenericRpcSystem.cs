using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DotsBridge
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class GenericRpcSystem<T> : SystemBase where T : unmanaged, IRpcCommand
    {
        public event Action<T, SingleEntity> OnReceived;
        private BridgeWorld _bridge;
        private EntityQuery _rpcQuery;

        protected override void OnCreate()
        {
            _rpcQuery = GetEntityQuery(
                ComponentType.ReadOnly<T>(),
                ComponentType.ReadOnly<ReceiveRpcCommandRequest>()
            );
            // Система засыпает, если нет данных, но просыпается автоматически
            RequireForUpdate(_rpcQuery);
        }

        protected override void OnUpdate()
        {
            if (_bridge == null)
                _bridge = EntityBridge.GetOrCreateBridge(World);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var entities = _rpcQuery.ToEntityArray(Allocator.Temp);
            var rpcData = _rpcQuery.ToComponentDataArray<T>(Allocator.Temp);
            var requests = _rpcQuery.ToComponentDataArray<ReceiveRpcCommandRequest>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                if (OnReceived != null && _bridge != null)
                {
                    OnReceived.Invoke(rpcData[i], new SingleEntity(requests[i].SourceConnection, _bridge));
                }
                else
                {
                    Debug.LogWarning($"<color=orange>[{World.Name}]</color> RPC {typeof(T).Name} получен, но на него никто не подписан! Уничтожаю.");
                }

                ecb.DestroyEntity(entities[i]);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}