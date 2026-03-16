using SimBridge.Core.Time;
using Unity.Entities;

namespace SimVent.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct TimeUpdateSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Получаем доступ к нашему синглтону времени
            if (SystemAPI.TryGetSingletonRW<SimulationTimeComponent>(out var timeData))
            {
                // Прибавляем строго 0.02 секунды каждый физический тик
                timeData.ValueRW.TotalTime += timeData.ValueRO.FixedStep;
            }
        }
    }
}