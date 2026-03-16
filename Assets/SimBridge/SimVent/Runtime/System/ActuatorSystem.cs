using Unity.Entities;
using Unity.Mathematics;
using SimVent.Components;
using SimBridge.Core.Time; // Подключаем наш менеджер времени

namespace SimVent.Systems
{
    // Система обновления исполнительных механизмов
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct ActuatorSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Получаем наш симуляционный шаг (например, 0.02 секунды)
            var timeComp = SystemAPI.GetSingleton<SimulationTimeComponent>();
            float deltaTime = timeComp.FixedStep;

            // 1. Job для обновления заслонок
            new UpdateDampersJob { DeltaTime = deltaTime }.ScheduleParallel();

            // 2. Job для обновления вентиляторов
            new UpdateFansJob { DeltaTime = deltaTime }.ScheduleParallel();
        }
    }

    public partial struct UpdateDampersJob : IJobEntity
    {
        public float DeltaTime;

        void Execute(ref DamperComponent damper)
        {
            if (damper.TransitTime <= 0) return; // Защита от деления на ноль

            // Насколько заслонка может сдвинуться за один тик
            float step = (1.0f / damper.TransitTime) * DeltaTime;

            // Плавно двигаем текущее положение к целевому
            damper.CurrentOpening = math.select(
                damper.CurrentOpening - step,
                damper.CurrentOpening + step,
                damper.TargetOpening > damper.CurrentOpening
            );

            // Ограничиваем пределы (Clamp) и убираем дребезг, если оказались близко к цели
            if (math.abs(damper.TargetOpening - damper.CurrentOpening) < step)
            {
                damper.CurrentOpening = damper.TargetOpening;
            }
            damper.CurrentOpening = math.clamp(damper.CurrentOpening, 0f, 1f);
        }
    }

    public partial struct UpdateFansJob : IJobEntity
    {
        public float DeltaTime;

        void Execute(ref FanComponent fan)
        {
            if (fan.RunCommand)
            {
                // Разгон
                float step = (1.0f / fan.SpinUpTime) * DeltaTime;
                fan.CurrentSpeed = math.min(1.0f, fan.CurrentSpeed + step);
            }
            else
            {
                // Выбег (торможение)
                float step = (1.0f / fan.SpinDownTime) * DeltaTime;
                fan.CurrentSpeed = math.max(0.0f, fan.CurrentSpeed - step);
            }
        }
    }
}