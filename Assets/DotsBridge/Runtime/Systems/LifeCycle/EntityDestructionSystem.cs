using Unity.Entities;
using Unity.NetCode;

namespace DotsBridge.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    public partial class EntityDestructionSystem : SystemBase
    {
        private BridgeRegistry _registry;

        protected override void OnCreate()
        {
            RequireForUpdate<DeathEvent>(); 

            if (World.IsServer()) _registry = EntityBridge.ServerRegistry;
            else if (World.IsClient()) _registry = EntityBridge.ClientRegistry;
        }

        protected override void OnUpdate()
        {
            if (_registry == null) return;

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(World.Unmanaged);

            foreach (var (deathEvent, entity) in SystemAPI.Query<RefRO<DeathEvent>>().WithEntityAccess())
            {
                // --- 1. Обрабатываем подписки OOP-моста (строго для этого мира!) ---
                if (_registry.OnDestroyEvents.TryGetValue(entity, out var action))
                {
                    action?.Invoke(entity);
                    _registry.OnDestroyEvents.Remove(entity);
                }

                // --- 2. Окончательно удаляем сущность из памяти ---
                ecb.DestroyEntity(entity);
            }
        }
    }
}