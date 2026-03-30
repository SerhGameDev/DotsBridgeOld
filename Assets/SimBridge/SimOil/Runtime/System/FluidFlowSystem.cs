using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using SimBridge.Core.Time;
using Unity.Collections;

namespace SimOil.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct FluidFlowSystem : ISystem
    {
        private ComponentLookup<FluidMixture> _mixtureLookup;
        private ComponentLookup<PumpData> _pumpLookup; 
        private ComponentLookup<FractionFilter> _filterLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationTimeComponent>();
            state.RequireForUpdate<SimulationOilControl>();
            
            _mixtureLookup = state.GetComponentLookup<FluidMixture>(isReadOnly: false);
            _pumpLookup = state.GetComponentLookup<PumpData>(isReadOnly: true);
            _filterLookup = state.GetComponentLookup<FractionFilter>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeData = SystemAPI.GetSingleton<SimulationTimeComponent>();
            var oilControl = SystemAPI.GetSingleton<SimulationOilControl>();
            
            if (!oilControl.IsRunning || timeData.TimeScale <= 0f) return;

            _mixtureLookup.Update(ref state);
            _pumpLookup.Update(ref state);
            _filterLookup.Update(ref state);

            var flowJob = new CalculateFlowJob
            {
                MixtureLookup = _mixtureLookup,
                PumpLookup = _pumpLookup,
                FilterLookup = _filterLookup,
                FixedStep = timeData.FixedStep
            };

            state.Dependency = flowJob.Schedule(state.Dependency);
        }

        [BurstCompile]
        private partial struct CalculateFlowJob : IJobEntity
        {
            public ComponentLookup<FluidMixture> MixtureLookup; 
            [ReadOnly] public ComponentLookup<PumpData> PumpLookup;
            [ReadOnly] public ComponentLookup<FractionFilter> FilterLookup;
            
            public float FixedStep;

            public void Execute(Entity entity, ref FluidLink link)
            {
                if (!MixtureLookup.HasComponent(link.NodeA) || !MixtureLookup.HasComponent(link.NodeB))
                    return;

                var mixtureA = MixtureLookup[link.NodeA];
                var mixtureB = MixtureLookup[link.NodeB];

                float pumpPressureBoost = 0f;
                if (PumpLookup.HasComponent(entity))
                {
                    var pump = PumpLookup[entity];
                    pumpPressureBoost = pump.MaxPressureBoost * pump.CurrentPower;
                }

                float effectivePressureA = mixtureA.Pressure + pumpPressureBoost;
                float deltaP = effectivePressureA - mixtureB.Pressure;
                float flowDirection = math.sign(deltaP);

                float targetFlowRate = math.sqrt(math.abs(deltaP)) * link.CrossSectionArea * flowDirection;
                link.CurrentFlowRateMass = targetFlowRate;

                float massToMove = targetFlowRate * FixedStep;

                // Проверка фильтрации
                FractionType filterType = FractionType.None;
                if (FilterLookup.HasComponent(entity))
                {
                    var filter = FilterLookup[entity];
                    // Если температура ниже точки кипения или поток обратный - блокируем трубу
                    if (mixtureA.Temperature < filter.MinBoilingTemperature || massToMove <= 0)
                    {
                        massToMove = 0;
                    }
                    else
                    {
                        filterType = filter.AllowedFraction;
                        // Нельзя выкачать больше фракции, чем есть в источнике
                        float availableFraction = mixtureA.TotalMass * GetFractionShare(in mixtureA, filterType);
                        massToMove = math.min(massToMove, availableFraction);
                    }
                }
                else
                {
                    // Обычная логика для труб без фильтров
                    if (massToMove > 0 && massToMove > mixtureA.TotalMass) massToMove = mixtureA.TotalMass;
                    if (massToMove < 0 && math.abs(massToMove) > mixtureB.TotalMass) massToMove = -mixtureB.TotalMass;
                }

                if (math.abs(massToMove) > 0.0001f)
                {
                    if (massToMove > 0) 
                    {
                        if (filterType != FractionType.None)
                        {
                            ExtractFraction(ref mixtureA, massToMove, filterType);
                            InjectFraction(ref mixtureB, in mixtureA, massToMove, filterType);
                        }
                        else
                        {
                            MixFluids(ref mixtureB, in mixtureA, massToMove);
                            mixtureA.TotalMass -= massToMove;
                        }
                    }
                    else 
                    {
                        float absMass = math.abs(massToMove);
                        MixFluids(ref mixtureA, in mixtureB, absMass);
                        mixtureB.TotalMass -= absMass;
                    }

                    MixtureLookup[link.NodeA] = mixtureA;
                    MixtureLookup[link.NodeB] = mixtureB;
                }
            }

            private void MixFluids(ref FluidMixture target, in FluidMixture source, float addedMass)
            {
                float newTotalMass = target.TotalMass + addedMass;
                if (newTotalMass <= 0.0001f) return;

                target.FractionGas = (target.FractionGas * target.TotalMass + source.FractionGas * addedMass) / newTotalMass;
                target.FractionLightNaphtha = (target.FractionLightNaphtha * target.TotalMass + source.FractionLightNaphtha * addedMass) / newTotalMass;
                target.FractionHeavyNaphtha = (target.FractionHeavyNaphtha * target.TotalMass + source.FractionHeavyNaphtha * addedMass) / newTotalMass;
                target.FractionKerosene = (target.FractionKerosene * target.TotalMass + source.FractionKerosene * addedMass) / newTotalMass;
                target.FractionLightDiesel = (target.FractionLightDiesel * target.TotalMass + source.FractionLightDiesel * addedMass) / newTotalMass;
                target.FractionHeavyDiesel = (target.FractionHeavyDiesel * target.TotalMass + source.FractionHeavyDiesel * addedMass) / newTotalMass;
                target.FractionMazut = (target.FractionMazut * target.TotalMass + source.FractionMazut * addedMass) / newTotalMass;
                target.FractionWater = (target.FractionWater * target.TotalMass + source.FractionWater * addedMass) / newTotalMass;

                target.Temperature = (target.Temperature * target.TotalMass + source.Temperature * addedMass) / newTotalMass;
                target.TotalMass = newTotalMass;
            }

            private float GetFractionShare(in FluidMixture mix, FractionType type)
            {
                return type switch
                {
                    FractionType.Gas => mix.FractionGas,
                    FractionType.LightNaphtha => mix.FractionLightNaphtha,
                    FractionType.HeavyNaphtha => mix.FractionHeavyNaphtha,
                    FractionType.Kerosene => mix.FractionKerosene,
                    FractionType.LightDiesel => mix.FractionLightDiesel,
                    FractionType.HeavyDiesel => mix.FractionHeavyDiesel,
                    FractionType.Mazut => mix.FractionMazut,
                    FractionType.Water => mix.FractionWater,
                    _ => 0f
                };
            }

            private void ExtractFraction(ref FluidMixture source, float massToRemove, FractionType type)
            {
                float newTotalMass = source.TotalMass - massToRemove;
                if (newTotalMass <= 0.0001f) { source.TotalMass = 0; return; }

                source.FractionGas = (source.FractionGas * source.TotalMass - (type == FractionType.Gas ? massToRemove : 0)) / newTotalMass;
                source.FractionLightNaphtha = (source.FractionLightNaphtha * source.TotalMass - (type == FractionType.LightNaphtha ? massToRemove : 0)) / newTotalMass;
                source.FractionHeavyNaphtha = (source.FractionHeavyNaphtha * source.TotalMass - (type == FractionType.HeavyNaphtha ? massToRemove : 0)) / newTotalMass;
                source.FractionKerosene = (source.FractionKerosene * source.TotalMass - (type == FractionType.Kerosene ? massToRemove : 0)) / newTotalMass;
                source.FractionLightDiesel = (source.FractionLightDiesel * source.TotalMass - (type == FractionType.LightDiesel ? massToRemove : 0)) / newTotalMass;
                source.FractionHeavyDiesel = (source.FractionHeavyDiesel * source.TotalMass - (type == FractionType.HeavyDiesel ? massToRemove : 0)) / newTotalMass;
                source.FractionMazut = (source.FractionMazut * source.TotalMass - (type == FractionType.Mazut ? massToRemove : 0)) / newTotalMass;
                source.FractionWater = (source.FractionWater * source.TotalMass - (type == FractionType.Water ? massToRemove : 0)) / newTotalMass;

                source.TotalMass = newTotalMass;
            }

            private void InjectFraction(ref FluidMixture target, in FluidMixture source, float addedMass, FractionType type)
            {
                float newTotalMass = target.TotalMass + addedMass;
                if (newTotalMass <= 0.0001f) return;

                target.FractionGas = (target.FractionGas * target.TotalMass + (type == FractionType.Gas ? addedMass : 0)) / newTotalMass;
                target.FractionLightNaphtha = (target.FractionLightNaphtha * target.TotalMass + (type == FractionType.LightNaphtha ? addedMass : 0)) / newTotalMass;
                target.FractionHeavyNaphtha = (target.FractionHeavyNaphtha * target.TotalMass + (type == FractionType.HeavyNaphtha ? addedMass : 0)) / newTotalMass;
                target.FractionKerosene = (target.FractionKerosene * target.TotalMass + (type == FractionType.Kerosene ? addedMass : 0)) / newTotalMass;
                target.FractionLightDiesel = (target.FractionLightDiesel * target.TotalMass + (type == FractionType.LightDiesel ? addedMass : 0)) / newTotalMass;
                target.FractionHeavyDiesel = (target.FractionHeavyDiesel * target.TotalMass + (type == FractionType.HeavyDiesel ? addedMass : 0)) / newTotalMass;
                target.FractionMazut = (target.FractionMazut * target.TotalMass + (type == FractionType.Mazut ? addedMass : 0)) / newTotalMass;
                target.FractionWater = (target.FractionWater * target.TotalMass + (type == FractionType.Water ? addedMass : 0)) / newTotalMass;

                target.Temperature = (target.Temperature * target.TotalMass + source.Temperature * addedMass) / newTotalMass;
                target.TotalMass = newTotalMass;
            }
        }
    }
}