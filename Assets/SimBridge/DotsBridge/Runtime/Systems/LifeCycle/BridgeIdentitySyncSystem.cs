using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace DotsBridge.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class BridgeIdentitySyncSystem : SystemBase
    {
        private EntityQuery _newEntitiesQuery;
        private EntityQuery _deadEntitiesQuery;

        protected override void OnCreate()
        {
            _newEntitiesQuery = SystemAPI.QueryBuilder().WithAll<BridgeIdentity>().WithNone<BridgeIdentityCleanup>().Build();
            _deadEntitiesQuery = SystemAPI.QueryBuilder().WithAll<BridgeIdentityCleanup>().WithNone<BridgeIdentity>().Build();
        }

        protected override void OnUpdate()
        {
            var registry = ClientBridge.World();
            if (registry == null) return;

            if (_newEntitiesQuery.IsEmptyIgnoreFilter && _deadEntitiesQuery.IsEmptyIgnoreFilter)
                return;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // РЕГИСТРАЦИЯ НОВЫХ
            foreach (var (identity, entity) in SystemAPI.Query<RefRO<BridgeIdentity>>().WithNone<BridgeIdentityCleanup>().WithEntityAccess())
            {
                int hash = identity.ValueRO.Hash;

                registry.AddEntityToGroup(hash, entity);

                ecb.AddComponent(entity, new BridgeIdentityCleanup { Hash = hash });
            }

            foreach (var (cleanup, entity) in SystemAPI.Query<RefRO<BridgeIdentityCleanup>>().WithNone<BridgeIdentity>().WithEntityAccess())
            {
                int hash = cleanup.ValueRO.Hash;

                registry.RemoveEntityFromGroup(hash, entity);

                ecb.RemoveComponent<BridgeIdentityCleanup>(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}