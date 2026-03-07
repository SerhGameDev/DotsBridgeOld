using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

namespace DotsBridge.Modules.Network.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ServerOopRpcReceiveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var rpcs = new NativeList<OopEventRpc>(Allocator.Temp);
            var entities = new NativeList<Entity>(Allocator.Temp);

            // МЫ ЗАПРАШИВАЕМ ReceiveRpcCommandRequest, чтобы узнать, откуда пришел пакет
            foreach (var (rpc, request, entity) in SystemAPI.Query<RefRW<OopEventRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                // Проверяем, есть ли у этого соединения NetworkId (рукопожатие пройдено)
                if (state.EntityManager.HasComponent<NetworkId>(request.ValueRO.SourceConnection))
                {
                    // Достаем НАСТОЯЩИЙ ID прямо из движка
                    var networkId = state.EntityManager.GetComponentData<NetworkId>(request.ValueRO.SourceConnection).Value;

                    var data = rpc.ValueRW;
                    // ПЕРЕЗАПИСЫВАЕМ IntValue настоящим ID (защита от читов)
                    data.IntValue = networkId;

                    rpcs.Add(data);
                }
                entities.Add(entity);
            }

            if (entities.Length > 0)
                state.EntityManager.DestroyEntity(entities.AsArray());

            foreach (var rpcData in rpcs)
            {
                OopRpcRegistry.InvokeOnServer(rpcData.EventHash, rpcData);
            }

            rpcs.Dispose();
            entities.Dispose();
        }
    }
}