using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace DotsBridge.Systems
{
    // Система должна работать в обоих мирах (и на сервере, и на клиенте)
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class BridgeIdentitySyncSystem : SystemBase
    {
        private BridgeState _worldState;

        protected override void OnCreate()
        {
            // При создании системы находим стейт именно этого мира
            if (World.IsServer()) _worldState = EntityBridge.ServerState;
            else if (World.IsClient()) _worldState = EntityBridge.ClientState;
            else _worldState = EntityBridge.DefaultState;
        }

        protected override void OnUpdate()
        {
            if (_worldState == null) return;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // ==========================================
            // 1. РОЖДЕНИЕ (Новые сущности)
            // Ищем тех, у кого ЕСТЬ Identity, но НЕТ Cleanup
            // ==========================================
            foreach (var (identity, entity) in SystemAPI.Query<RefRO<BridgeIdentity>>().WithNone<BridgeIdentityCleanup>().WithEntityAccess())
            {
                int hash = identity.ValueRO.Hash;

                // Добавляем в наш OOP-мост
                _worldState.AddEntityToContainer(hash, entity);

                // Вешаем тень (теперь система знает, что сущность учтена)
                ecb.AddComponent(entity, new BridgeIdentityCleanup { Hash = hash });
            }

            // ==========================================
            // 2. СМЕРТЬ (Удаленные сущности)
            // Ищем тех, у кого ОСТАЛСЯ Cleanup, но УЖЕ НЕТ Identity
            // ==========================================
            foreach (var (cleanup, entity) in SystemAPI.Query<RefRO<BridgeIdentityCleanup>>().WithNone<BridgeIdentity>().WithEntityAccess())
            {
                int hash = cleanup.ValueRO.Hash;

                // Удаляем из нашего OOP-моста
                _worldState.RemoveEntityFromContainer(hash, entity);

                // Снимаем тень, чтобы движок смог окончательно удалить сущность из памяти
                ecb.RemoveComponent<BridgeIdentityCleanup>(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}