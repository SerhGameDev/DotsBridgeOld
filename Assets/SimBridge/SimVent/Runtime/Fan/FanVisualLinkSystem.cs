using DotsBridge;
using DotsBridge.Systems;
using SimVent.Components;
using Unity.Burst;
using Unity.Entities;

namespace SimVent
{
    [UpdateBefore(typeof(ToggleRotationSystem))]
    [BurstCompile]
    public partial struct FanVisualLinkSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Получаем доступ к логике вентиляторов (только чтение)
            var fanLookup = SystemAPI.GetComponentLookup<FanComponent>(true);

            // Перебираем все сущности, у которых есть мост-связка и визуальное вращение
            foreach (var (link, rotation) in SystemAPI.Query<RefRO<FanVisualLink>, RefRW<ToggleRotation>>())
            {
                // Проверяем, существует ли еще логический вентилятор
                if (fanLookup.TryGetComponent(link.ValueRO.LogicalFan, out var fanLogic))
                {
                    // Синхронизируем сигнал включения (если есть скорость или команда)
                    rotation.ValueRW.IsOn = fanLogic.RunCommand || fanLogic.CurrentSpeed > 0f;
                    
                    // Синхронизируем и умножаем скорость
                    rotation.ValueRW.Speed = fanLogic.CurrentSpeed * link.ValueRO.SpeedMultiplier;
                }
            }
        }
    }
}