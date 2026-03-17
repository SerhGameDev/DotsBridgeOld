using Unity.Entities;
using Unity.Mathematics;
using SimVent.Components;
using SimBridge.Core.Time;

namespace SimVent.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(DuctPhysicsSystem))] // Сначала вычисляем мощность, потом физику воздуха
    public partial struct HeaterActuatorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationTimeComponent>();
        }
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<SimulationTimeComponent>(out var time)) 
                return;

            float dt = time.FixedStep;
            var ductLookup = SystemAPI.GetComponentLookup<AirDuctComponent>(true);

            new HeaterControlJob
            {
                DuctLookup = ductLookup,
                DeltaTime = dt
            }.ScheduleParallel();
        }
    }

    public partial struct HeaterControlJob : IJobEntity
    {
        [Unity.Collections.ReadOnly] public ComponentLookup<AirDuctComponent> DuctLookup;
        public float DeltaTime;

        void Execute(ref ElectricHeaterComponent heater)
        {
            if (!DuctLookup.HasComponent(heater.TargetDuct)) return;

            var duct = DuctLookup[heater.TargetDuct];
            float currentDuctTemp = duct.AirTemperature;
            float currentFlow = duct.CurrentFlowRate;

            // 1. ПРОВЕРКА ПЕРЕГРЕВА (Защита)
            if (currentDuctTemp > heater.OverheatThreshold)
            {
                heater.IsOverheated = true;
            }

            bool isFlowOk = currentFlow > 100f;

            float targetKW = math.saturate(heater.TargetPower) * heater.MaxPowerKW;

            // Условия отключения: Авария ИЛИ отсутствие потока
            if (heater.IsOverheated || !isFlowOk)
            {
                targetKW = 0f;
            }

            // 4. ИНЕРЦИЯ (Плавный нагрев)
            float speed = DeltaTime / math.max(1f, heater.HeatingTimeConstant);
            heater.CurrentHeatOutput = math.lerp(heater.CurrentHeatOutput, targetKW, speed);
        }
    }
}