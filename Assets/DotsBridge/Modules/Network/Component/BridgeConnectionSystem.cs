using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace DotsBridge.Modules.Network
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class BridgeConnectionSystem : SystemBase
    {
        private BridgeWorld _bridge;

        protected override void OnUpdate()
        {
            // Инициализируем мост текущего мира
            if (_bridge == null)
            {
                _bridge = EntityBridge.InServerWorld();
                if (_bridge == null) return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (netId, entity) in SystemAPI.Query<RefRO<NetworkId>>()
                         .WithNone<BridgeClientCleanup>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new BridgeClientCleanup { NetworkId = netId.ValueRO.Value });

                var connectionFacade = new SingleEntity(entity, _bridge);
                EntityBridge.InvokeClientConnected(connectionFacade, netId.ValueRO.Value);
            }

            foreach (var (cleanup, entity) in SystemAPI.Query<RefRO<BridgeClientCleanup>>()
                         .WithNone<NetworkId>()
                         .WithEntityAccess())
            {
                ecb.RemoveComponent<BridgeClientCleanup>(entity);
                EntityBridge.InvokeClientDisconnected(cleanup.ValueRO.NetworkId);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}