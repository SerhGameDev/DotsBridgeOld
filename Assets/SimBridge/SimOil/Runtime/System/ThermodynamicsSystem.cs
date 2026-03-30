using Unity.Burst;
using Unity.Entities;
using SimBridge.Core.Time;
using SimOil.Equipment;
using Unity.Collections;

namespace SimOil.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(FluidFlowSystem))] // Нагреваем перед тем, как перекачивать
    [BurstCompile]
    public partial struct ThermodynamicsSystem : ISystem
    {
        // Средняя теплоемкость нефтепродуктов (~2.0 кДж на кг на градус)
        private const float OilSpecificHeat = 2.0f; 
        
        // Lookup для проверки наличия печи на узле
        private ComponentLookup<HeaterData> _heaterLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationTimeComponent>();
            state.RequireForUpdate<SimulationOilControl>();
            
            // Инициализируем Lookup только для чтения
            _heaterLookup = state.GetComponentLookup<HeaterData>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeData = SystemAPI.GetSingleton<SimulationTimeComponent>();
            var oilControl = SystemAPI.GetSingleton<SimulationOilControl>();
            
            if (!oilControl.IsRunning || timeData.TimeScale <= 0f) return;

            // Обновляем указатели на память
            _heaterLookup.Update(ref state);

            var heatJob = new CalculateHeatJob
            {
                FixedStep = timeData.FixedStep,
                AmbientTemp = oilControl.AmbientTemperature,
                HeaterLookup = _heaterLookup
            };

            // Запускаем расчет параллельно по всем узлам
            state.Dependency = heatJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct CalculateHeatJob : IJobEntity
        {
            public float FixedStep;
            public float AmbientTemp;
            
            [ReadOnly] 
            public ComponentLookup<HeaterData> HeaterLookup;

            // Добавили Entity в параметры для доступа к Lookup
            public void Execute(Entity entity, ref FluidMixture mixture)
            {
                if (mixture.TotalMass <= 0.001f) return;

                float currentTemp = mixture.Temperature;

                // 1. АКТИВНЫЙ НАГРЕВ (Если на узле есть HeaterData)
                if (HeaterLookup.HasComponent(entity))
                {
                    var heater = HeaterLookup[entity];
                    
                    // Энергия в килоджоулях (кВт * секунды)
                    float energyAdded = heater.MaxHeatPower * heater.CurrentPower * FixedStep;
                    
                    // Повышение температуры (dT = Q / (m * C))
                    float tempIncrease = energyAdded / (mixture.TotalMass * OilSpecificHeat);
                    currentTemp += tempIncrease;
                }

                // 2. ПОТЕРИ В ОКРУЖАЮЩУЮ СРЕДУ (Остывание)
                float heatLossFactor = HeaterLookup.HasComponent(entity) ? HeaterLookup[entity].HeatLossFactor : 0.05f; 
                
                if (currentTemp > AmbientTemp)
                {
                    // Закон Ньютона: dT = -k * (T - T_env) * dt
                    float tempLoss = heatLossFactor * (currentTemp - AmbientTemp) * FixedStep;
                    currentTemp -= tempLoss;
                }

                mixture.Temperature = currentTemp;
            }
        }
    }
}