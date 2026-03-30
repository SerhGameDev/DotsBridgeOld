using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace SimOil.Systems
{
    // Система работает только на сервере
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
    public partial struct ServerFluidSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Мы ищем все сущности, на которых есть и физика, и сеть
            foreach (var (physics, sync) in SystemAPI.Query<RefRO<FluidMixture>, RefRW<NetSync_FluidNode>>())
            {
                // Перекладываем данные из локальной физики в сетевой Ghost-компонент
                sync.ValueRW.TotalMass = physics.ValueRO.TotalMass;
                sync.ValueRW.Pressure = physics.ValueRO.Pressure;
                sync.ValueRW.Temperature = physics.ValueRO.Temperature;
            }
        }
    }
}