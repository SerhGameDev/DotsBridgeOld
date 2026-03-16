using Unity.Entities;
using Unity.Mathematics;
using SimVent.Components;
using SimBridge.Core.Time; // Наш менеджер времени

namespace SimVent.Systems
{
    // Датчики обновляются в самом конце кадра симуляции, когда потоки уже посчитаны
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct SensorUpdateSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Берем текущее время симуляции
            var timeComp = SystemAPI.GetSingleton<SimulationTimeComponent>();
            double currentTime = timeComp.TotalTime;

            var ductLookup = SystemAPI.GetComponentLookup<AirDuctComponent>(true);

            new UpdateDpsSensorsJob
            {
                Time = (float)currentTime,
                DuctLookup = ductLookup
            }.ScheduleParallel();
        }
    }

    public partial struct UpdateDpsSensorsJob : IJobEntity
    {
        public float Time;
        [Unity.Collections.ReadOnly] public ComponentLookup<AirDuctComponent> DuctLookup;

        void Execute(ref DpsSensorComponent dps)
        {
            if (!DuctLookup.HasComponent(dps.TargetDuct))
                return;

            float actualFlow = DuctLookup[dps.TargetDuct].CurrentFlowRate;

            // Если вентилятор выключен (поток около нуля), дребезга нет
            if (actualFlow < 10f)
            {
                dps.OutputSignal = false;
                return;
            }

            // Генерируем псевдослучайный "аэродинамический шум"
            // Комбинация двух синусоид с некратными высокими частотами дает эффект "рваного" потока
            float noise = (math.sin(Time * 43.0f) * 0.6f + math.cos(Time * 71.0f) * 0.4f) * dps.FlickerBand;

            // "Воспринимаемое" датчиком давление/поток в этот момент времени
            float perceivedFlow = actualFlow + noise;

            // Датчик замыкает контакт, если текущий зашумленный поток превысил уставку
            dps.OutputSignal = perceivedFlow > dps.Setpoint;
        }
    }
}