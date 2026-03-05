using Unity.Entities;

namespace DotsBridge.Systems
{
    // OrderLast = true гарантирует, что эта система выполнится самой последней в группе LateSimulation
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    public partial class EntityDestructionSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<DeathEvent>();
        }

        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(World.Unmanaged);

            // Этот Query вернет ТОЛЬКО те сущности, у которых DeathEvent ВКЛЮЧЕН
            foreach (var (deathEvent, entity) in SystemAPI.Query<RefRO<DeathEvent>>().WithEntityAccess())
            {
                // --- 1. Обрабатываем подписки OOP-моста ---
                if (EntityBridge.OnDestroyEvents.TryGetValue(entity, out var action))
                {
                    action?.Invoke(entity);
                    EntityBridge.OnDestroyEvents.Remove(entity);
                }

                // --- 2. Окончательно удаляем сущность из памяти ---
                ecb.DestroyEntity(entity);
            }
        }
    }
}