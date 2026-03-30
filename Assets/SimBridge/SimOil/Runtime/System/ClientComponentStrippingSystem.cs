using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode; // Обязательно!

namespace SimOil.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct ClientComponentStrippingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Используем наиболее совместимый способ проверки через флаги
            var IsClient = (state.World.Flags & WorldFlags.GameClient) != 0;

            if (IsClient) 
            {
                var ecb = new EntityCommandBuffer(Allocator.Temp);
                
                // Ищем все сущности с FluidMixture
                // Обрати внимание: Query должен быть эффективным
                foreach (var (physics, entity) in SystemAPI.Query<FluidMixture>().WithEntityAccess())
                {
                    ecb.RemoveComponent<FluidMixture>(entity);
                }

                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }
            
            state.Enabled = false; 
        }
    }
}