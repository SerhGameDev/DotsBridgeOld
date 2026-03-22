using Unity.Entities;
using Unity.Mathematics;
using SimVent.Components;
using SimBridge.Core.Time;

namespace SimVent.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(ActuatorSystem))]
    public partial struct DuctPhysicsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationTimeComponent>();
        }
        public void OnUpdate(ref SystemState state)
        {
            // Получаем шаг времени
            float dt = SystemAPI.GetSingleton<SimulationTimeComponent>().FixedStep;

            // Получаем наш КОНФИГ
            if (!SystemAPI.TryGetSingleton<SimVentConfigComponent>(out var config))
            {
                // Если конфига нет на сцене, выходим, чтобы не было ошибок
                return;
            }

            var nodeLookup = SystemAPI.GetComponentLookup<AirNodeComponent>(true);
            var stateLookup = SystemAPI.GetComponentLookup<DuctStateComponent>(false);

            // --- ЭТАП 1: СБРОС БЛОКНОТА С ДИНАМИЧЕСКИМ СОПРОТИВЛЕНИЕМ ---
            new ResetDuctStateJob
            {
                NodeLookup = nodeLookup,
                Config = config
            }.ScheduleParallel();

            state.Dependency.Complete();

            // --- ЭТАП 2: СБОР ДАННЫХ С ОБОРУДОВАНИЯ ---
            new ApplyFansJob { StateLookup = stateLookup }.Schedule();
            new ApplyDampersJob { StateLookup = stateLookup }.Schedule();
            new ApplyHeatersJob { StateLookup = stateLookup }.Schedule();
            new ApplyFiltersJob { StateLookup = stateLookup }.Schedule();
            state.Dependency.Complete();

            // --- ЭТАП 3: РАСЧЕТ ПОТОКА ---
            new CalculateFlowJob { NodeLookup = nodeLookup }.ScheduleParallel();
            state.Dependency.Complete();

            // --- ЭТАП 4: РАСЧЕТ УЗЛОВ И ТЕМПЕРАТУРЫ ---
            var ducts = SystemAPI.QueryBuilder().WithAll<AirDuctComponent>().Build()
                .ToComponentDataArray<AirDuctComponent>(Unity.Collections.Allocator.TempJob);

            new UpdateNodesAndTempJob
            {
                AllDucts = ducts,
                DeltaTime = dt,
                Config = config // Передаем конфиг
            }.ScheduleParallel();

            state.Dependency.Complete();

            // --- ЭТАП 5: ОБНОВЛЕНИЕ ТЕМПЕРАТУРЫ В ТРУБЕ ---
            new UpdateDuctTempJob
            {
                NodeLookup = nodeLookup,
                DeltaTime = dt,
                Config = config // Передаем конфиг
            }.ScheduleParallel();
        }
    }

    // ==========================================
    // ДЖОБЫ (КОНВЕЙЕР ФИЗИКИ)
    // ==========================================

    public partial struct ResetDuctStateJob : IJobEntity
    {
        [Unity.Collections.ReadOnly] public ComponentLookup<AirNodeComponent> NodeLookup;
        public SimVentConfigComponent Config;

        void Execute(in AirDuctComponent duct, ref DuctStateComponent state)
        {
            state.FanPressureBoost = 0f;
            state.TotalHeatKW = 0f;

            float nodeResistanceContribution = 0f;
        
            // Считаем сопротивление, только если источник - НЕ бесконечная улица
            if (NodeLookup.HasComponent(duct.SourceNode))
            {
                var node = NodeLookup[duct.SourceNode];
                // Если это обычная промежуточная труба (не улица), берем её длину
                if (!node.IsInfinite) 
                {
                    nodeResistanceContribution = node.Volume * Config.DuctFrictionPerMeter;
                }
            }

            state.TotalResistance = Config.BaseDuctResistance + nodeResistanceContribution;
        }
    }

    public partial struct ApplyFansJob : IJobEntity
    {
        public ComponentLookup<DuctStateComponent> StateLookup;
        void Execute(in FanComponent fan)
        {
            if (!StateLookup.HasComponent(fan.TargetDuct)) return;
            var state = StateLookup[fan.TargetDuct];
            state.FanPressureBoost += fan.CurrentSpeed * fan.MaxPressure;
            StateLookup[fan.TargetDuct] = state;
        }
    }

    public partial struct ApplyDampersJob : IJobEntity
    {
        public ComponentLookup<DuctStateComponent> StateLookup;
        void Execute(in DamperComponent damper)
        {
            if (!StateLookup.HasComponent(damper.TargetDuct)) return;
            var state = StateLookup[damper.TargetDuct];
            state.TotalResistance /= math.max(0.01f, damper.CurrentOpening);
            StateLookup[damper.TargetDuct] = state;
        }
    }

    public partial struct ApplyHeatersJob : IJobEntity
    {
        public ComponentLookup<DuctStateComponent> StateLookup;
        void Execute(in ElectricHeaterComponent heater)
        {
            if (!StateLookup.HasComponent(heater.TargetDuct)) return;
            var state = StateLookup[heater.TargetDuct];
            state.TotalHeatKW += heater.CurrentHeatOutput;
            StateLookup[heater.TargetDuct] = state;
        }
    }

    public partial struct ApplyFiltersJob : IJobEntity
    {
        public ComponentLookup<DuctStateComponent> StateLookup;
        void Execute(in FilterComponent filter)
        {
            if (!StateLookup.HasComponent(filter.TargetDuct)) return;
            var state = StateLookup[filter.TargetDuct];
            float totalFilterRes = filter.NominalResistance + (filter.Dirtiness * filter.MaxDirtinessResistance);
            state.TotalResistance += totalFilterRes;
            StateLookup[filter.TargetDuct] = state;
        }
    }

    public partial struct CalculateFlowJob : IJobEntity
    {
        [Unity.Collections.ReadOnly] public ComponentLookup<AirNodeComponent> NodeLookup;
        void Execute(ref AirDuctComponent duct, in DuctStateComponent state)
        {
            if (!NodeLookup.HasComponent(duct.SourceNode) || !NodeLookup.HasComponent(duct.TargetNode)) return;

            float pSource = NodeLookup[duct.SourceNode].Pressure;
            float pTarget = NodeLookup[duct.TargetNode].Pressure;
            float flow = (pSource - pTarget + state.FanPressureBoost) / state.TotalResistance;
            duct.CurrentFlowRate = math.max(0f, flow);
        }
    }

    public partial struct UpdateNodesAndTempJob : IJobEntity
    {
        public float DeltaTime;
        public SimVentConfigComponent Config;
        [Unity.Collections.ReadOnly][Unity.Collections.DeallocateOnJobCompletion] public Unity.Collections.NativeArray<AirDuctComponent> AllDucts;

        void Execute(Entity nodeEntity, ref AirNodeComponent node)
        {
            if (node.IsInfinite)
            {
                node.Pressure = 0f;
                return;
            }

            float netFlow = 0f;
            float totalIncomingVolume = 0f;
            float weightedTempSum = 0f;

            for (int i = 0; i < AllDucts.Length; i++)
            {
                if (AllDucts[i].TargetNode == nodeEntity)
                {
                    netFlow += AllDucts[i].CurrentFlowRate;
                    float flowPerSec = (AllDucts[i].CurrentFlowRate / 3600f) * DeltaTime;
                    totalIncomingVolume += flowPerSec;
                    weightedTempSum += AllDucts[i].AirTemperature * flowPerSec;
                }
                if (AllDucts[i].SourceNode == nodeEntity)
                {
                    netFlow -= AllDucts[i].CurrentFlowRate;
                }
            }

            float safeVolume = math.max(0.1f, node.Volume);
            float netFlowPerSec = netFlow / 3600f;
            float volumeChange = netFlowPerSec * DeltaTime;

            // ИСПОЛЬЗУЕМ КОНФИГ
            float pressureDelta = (volumeChange / safeVolume) * Config.PressureMultiplier;
            node.Pressure += pressureDelta;

            node.Pressure = math.lerp(node.Pressure, 0f, DeltaTime * node.LeakFactor);

            float heatLoss = (node.Temperature - node.TargetStreetTemp) * node.HeatLossFactor * DeltaTime;
            node.Temperature -= heatLoss;

            if (totalIncomingVolume > 0)
            {
                totalIncomingVolume = math.min(totalIncomingVolume, safeVolume);
                float roomKeepRatio = (safeVolume - totalIncomingVolume) / safeVolume;
                node.Temperature = (node.Temperature * roomKeepRatio) + (weightedTempSum / safeVolume);
            }
        }
    }

    public partial struct UpdateDuctTempJob : IJobEntity
    {
        public float DeltaTime;
        public SimVentConfigComponent Config;
        [Unity.Collections.ReadOnly] public ComponentLookup<AirNodeComponent> NodeLookup;

        void Execute(ref AirDuctComponent duct, in DuctStateComponent state)
        {
            if (!NodeLookup.HasComponent(duct.SourceNode)) return;

            float inletTemp = NodeLookup[duct.SourceNode].Temperature;

            if (duct.CurrentFlowRate > 1f)
            {
                // ИСПОЛЬЗУЕМ КОНФИГ ДЛЯ ТЕПЛОЕМКОСТИ
                float deltaT = (state.TotalHeatKW * Config.AirHeatCapacityConstant) / duct.CurrentFlowRate;
                float targetTemp = inletTemp + deltaT;

                float ventSpeed = math.clamp((duct.CurrentFlowRate / 1000f) * DeltaTime, 0.1f, 1f);
                duct.AirTemperature = math.lerp(duct.AirTemperature, targetTemp, ventSpeed);
            }
            else
            {
                if (state.TotalHeatKW > 0)
                {
                    // ИСПОЛЬЗУЕМ КОНФИГ ДЛЯ ПЕРЕГРЕВА
                    duct.AirTemperature += (state.TotalHeatKW * Config.DeadHeadOverheatRate) * DeltaTime;
                }
                else
                {
                    // ИСПОЛЬЗУЕМ КОНФИГ ДЛЯ ОСТЫВАНИЯ
                    duct.AirTemperature = math.lerp(duct.AirTemperature, inletTemp, DeltaTime * Config.DeadHeadCooldownRate);
                }
            }
        }
    }
}