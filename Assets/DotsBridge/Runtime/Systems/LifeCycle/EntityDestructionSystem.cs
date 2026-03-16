using Unity.Entities;
using Unity.NetCode;

namespace DotsBridge.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    public partial class EntityDestructionSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<DeathEvent>();
        }

        protected override void OnUpdate()
        {
            var registry = EntityBridge.InCurrentWorld();
            if (registry == null) return;

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(World.Unmanaged);

            foreach (var (deathEvent, entity) in SystemAPI.Query<RefRO<DeathEvent>>().WithEntityAccess())
            {
                if (!SystemAPI.IsComponentEnabled<DeathEvent>(entity)) continue;

                if (registry.OnDestroyEvents.TryGetValue(entity, out var action))
                {
                    action?.Invoke(entity);
                    registry.OnDestroyEvents.Remove(entity);
                }

                ecb.DestroyEntity(entity);
            }
        }
    }
}