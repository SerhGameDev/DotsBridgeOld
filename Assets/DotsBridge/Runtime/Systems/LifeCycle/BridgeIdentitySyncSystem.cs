using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace DotsBridge.Systems
{
    // ====================================================================
    // ВОТ ОНО! Эта строчка заставляет систему работать в мультиплеере
    // ====================================================================
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class BridgeIdentitySyncSystem : SystemBase
    {
        private BridgeRegistry _registry;
        private EntityQuery _newEntitiesQuery;
        private EntityQuery _deadEntitiesQuery;

        protected override void OnCreate()
        {
            // Собираем запросы
            _newEntitiesQuery = SystemAPI.QueryBuilder().WithAll<BridgeIdentity>().WithNone<BridgeIdentityCleanup>().Build();
            _deadEntitiesQuery = SystemAPI.QueryBuilder().WithAll<BridgeIdentityCleanup>().WithNone<BridgeIdentity>().Build();

            UnityEngine.Debug.Log($"[DotsBridge] SyncSystem запущена в мире: {World.Name}");
        }

        protected override void OnUpdate()
        {
            // Ленивая инициализация реестра (ждем, пока NetworkManager его создаст)
            if (_registry == null)
            {
                if (World.IsServer()) _registry = EntityBridge.ServerRegistry;
                else if (World.IsClient()) _registry = EntityBridge.ClientRegistry;

                if (_registry == null) return; // Ждем дальше
            }

            // Ранний выход
            if (_newEntitiesQuery.IsEmptyIgnoreFilter && _deadEntitiesQuery.IsEmptyIgnoreFilter)
                return;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // 1. РОЖДЕНИЕ
            if (!_newEntitiesQuery.IsEmptyIgnoreFilter)
            {
                foreach (var (identity, entity) in SystemAPI.Query<RefRO<BridgeIdentity>>().WithNone<BridgeIdentityCleanup>().WithEntityAccess())
                {
                    int hash = identity.ValueRO.Hash;
                    _registry.AddEntityToGroup(hash, entity);
                    ecb.AddComponent(entity, new BridgeIdentityCleanup { Hash = hash });
                }
            }

            // 2. СМЕРТЬ
            if (!_deadEntitiesQuery.IsEmptyIgnoreFilter)
            {
                foreach (var (cleanup, entity) in SystemAPI.Query<RefRO<BridgeIdentityCleanup>>().WithNone<BridgeIdentity>().WithEntityAccess())
                {
                    int hash = cleanup.ValueRO.Hash;
                    _registry.RemoveEntityFromGroup(hash, entity);
                    ecb.RemoveComponent<BridgeIdentityCleanup>(entity);
                }
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}