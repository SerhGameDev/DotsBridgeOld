using Unity.Entities;
using Unity.Mathematics;
using SimVent.Components;
using SimBridge.Core.Time;

namespace SimVent.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(SensorUpdateSystem))]
    public partial struct ThermodynamicsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationTimeComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.GetSingleton<SimulationTimeComponent>().FixedStep;

            // 1. Считаем нагрев ТЭНа и температуру воздуха ВНУТРИ труб
            var nodeLookup = SystemAPI.GetComponentLookup<AirNodeComponent>(true);
            new CalculateDuctPhysicsJob
            {
                DeltaTime = dt,
                NodeLookup = nodeLookup
            }.ScheduleParallel();

            state.Dependency.Complete();

            // ИСПРАВЛЕНИЕ: Используем ToComponentDataArray вместо ToComponentArray
            var ductQuery = SystemAPI.QueryBuilder().WithAll<AirDuctComponent>().Build();
            var ducts = ductQuery.ToComponentDataArray<AirDuctComponent>(Unity.Collections.Allocator.TempJob);

            // 2. Обновляем температуру в УЗЛАХ (смешивание в комнатах)
            new UpdateNodesTemperatureJob
            {
                DeltaTime = dt,
                AllDucts = ducts
            }.ScheduleParallel();

            // 3. Обновляем датчики температуры (ИСПРАВЛЕНИЕ: читаем из Duct, а не из Node)
            var ductLookupForSensors = SystemAPI.GetComponentLookup<AirDuctComponent>(true);
            new UpdateTempSensorsJob
            {
                DeltaTime = dt,
                DuctLookup = ductLookupForSensors
            }.ScheduleParallel();
        }
    }

    public partial struct CalculateDuctPhysicsJob : IJobEntity
    {
        public float DeltaTime;
        [Unity.Collections.ReadOnly] public ComponentLookup<AirNodeComponent> NodeLookup;

        void Execute(ref AirDuctComponent duct, ref ElectricHeaterComponent heater)
        {
            if (!NodeLookup.HasComponent(duct.SourceNode)) return;

            float targetKw = heater.IsOverheated ? 0f : heater.TargetPower * heater.MaxPowerKW;
            heater.CurrentHeatOutput += (targetKw - heater.CurrentHeatOutput) * (DeltaTime / heater.HeatingTimeConstant);

            float inletTemp = NodeLookup[duct.SourceNode].Temperature;

            if (duct.CurrentFlowRate > 10f)
            {
                float deltaT = (heater.CurrentHeatOutput * 2985f) / duct.CurrentFlowRate;
                duct.AirTemperature = inletTemp + deltaT;
            }
            else
            {
                duct.AirTemperature = inletTemp + (heater.CurrentHeatOutput * 100f);
            }

            if (duct.AirTemperature >= heater.OverheatThreshold) heater.IsOverheated = true;
        }
    }

    public partial struct UpdateNodesTemperatureJob : IJobEntity
    {
        public float DeltaTime;

        // ДОБАВЛЯЕМ [ReadOnly] - это решает проблему с доступом в Parallel Job
        [Unity.Collections.ReadOnly]
        [Unity.Collections.DeallocateOnJobCompletion]
        public Unity.Collections.NativeArray<AirDuctComponent> AllDucts;

        void Execute(Entity nodeEntity, ref AirNodeComponent node)
        {
            if (node.IsInfinite) return;

            // 1. Теплопотери через стены
            float heatLoss = (node.Temperature - node.TargetStreetTemp) * node.HeatLossFactor * DeltaTime;
            node.Temperature -= heatLoss;

            float totalIncomingVolume = 0;
            float weightedTempSum = 0;

            // Теперь этот цикл будет работать в параллельном режиме без ошибок
            for (int i = 0; i < AllDucts.Length; i++)
            {
                if (AllDucts[i].TargetNode == nodeEntity && AllDucts[i].CurrentFlowRate > 0)
                {
                    float flowPerSec = (AllDucts[i].CurrentFlowRate / 3600f) * DeltaTime;
                    totalIncomingVolume += flowPerSec;
                    weightedTempSum += AllDucts[i].AirTemperature * flowPerSec;
                }
            }

            if (totalIncomingVolume > 0)
            {
                totalIncomingVolume = math.min(totalIncomingVolume, node.Volume);
                float roomKeepRatio = (node.Volume - totalIncomingVolume) / node.Volume;
                node.Temperature = (node.Temperature * roomKeepRatio) + (weightedTempSum / node.Volume);
            }
        }
    }

    // ИСПРАВЛЕНИЕ: датчик теперь смотрит на трубу, а не на узел
    public partial struct UpdateTempSensorsJob : IJobEntity
    {
        public float DeltaTime;
        [Unity.Collections.ReadOnly] public ComponentLookup<AirDuctComponent> DuctLookup;

        void Execute(ref TemperatureSensorComponent sensor)
        {
            if (!DuctLookup.HasComponent(sensor.TargetDuct)) return;
            float targetTemp = DuctLookup[sensor.TargetDuct].AirTemperature;
            sensor.MeasuredTemperature += (targetTemp - sensor.MeasuredTemperature) * (DeltaTime / sensor.SensorTimeConstant);
        }
    }
}