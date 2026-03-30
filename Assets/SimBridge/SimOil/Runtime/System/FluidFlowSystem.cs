using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using SimBridge.Core.Time;
using Unity.Collections;
using UnityEngine;

namespace SimOil.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct FluidFlowSystem : ISystem
    {
        private ComponentLookup<FluidMixture> _mixtureLookup;

        private ComponentLookup<PumpData> _pumpLookup; 

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationTimeComponent>();
            state.RequireForUpdate<SimulationOilControl>();
            
            _mixtureLookup = state.GetComponentLookup<FluidMixture>(isReadOnly: false);
            _pumpLookup = state.GetComponentLookup<PumpData>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeData = SystemAPI.GetSingleton<SimulationTimeComponent>();
            var oilControl = SystemAPI.GetSingleton<SimulationOilControl>();
            
            if (!oilControl.IsRunning || timeData.TimeScale <= 0f) return;

            _mixtureLookup.Update(ref state);
            _pumpLookup.Update(ref state);

            var flowJob = new CalculateFlowJob
            {
                MixtureLookup = _mixtureLookup,
                PumpLookup = _pumpLookup,
                FixedStep = timeData.FixedStep
            };

            state.Dependency = flowJob.Schedule(state.Dependency);
        }
        [BurstCompile]
        private partial struct CalculateFlowJob : IJobEntity
        {
            // Здесь мы пишем и читаем, поэтому атрибут не нужен
            public ComponentLookup<FluidMixture> MixtureLookup; 
            
            // ДОБАВЛЕН АТРИБУТ [ReadOnly]
            [ReadOnly] 
            public ComponentLookup<PumpData> PumpLookup;
            
            public float FixedStep;

            public void Execute(Entity entity, ref FluidLink link)
            {
                if (!MixtureLookup.HasComponent(link.NodeA) || !MixtureLookup.HasComponent(link.NodeB))
                    return;

                var mixtureA = MixtureLookup[link.NodeA];
                var mixtureB = MixtureLookup[link.NodeB];

                // 1. Учет насоса. Если на трубе есть насос, он искусственно повышает давление в узле А (выталкивает)
                float pumpPressureBoost = 0f;
                if (PumpLookup.HasComponent(entity))
                {
                    var pump = PumpLookup[entity];
                    pumpPressureBoost = pump.MaxPressureBoost * pump.CurrentPower;
                }

                // Эффективное давление: реальное + напор насоса
                float effectivePressureA = mixtureA.Pressure + pumpPressureBoost;
                
                float deltaP = effectivePressureA - mixtureB.Pressure;
                float flowDirection = math.sign(deltaP);

                // Расчет расхода
                float targetFlowRate = math.sqrt(math.abs(deltaP)) * link.CrossSectionArea * flowDirection;
                link.CurrentFlowRateMass = targetFlowRate;

                float massToMove = targetFlowRate * FixedStep;

                // Защита от отрицательной массы
                if (massToMove > 0 && massToMove > mixtureA.TotalMass) massToMove = mixtureA.TotalMass;
                if (massToMove < 0 && math.abs(massToMove) > mixtureB.TotalMass) massToMove = -mixtureB.TotalMass;

                // 2. Смешивание фракций (Перенос массы)
                if (math.abs(massToMove) > 0.0001f)
                {
                    if (massToMove > 0) 
                    {
                        // Течет от A к B
                        MixFluids(ref mixtureB, in mixtureA, massToMove);
                        mixtureA.TotalMass -= massToMove;
                    }
                    else 
                    {
                        // Течет от B к A (обратный ток, если насос выключен, а узел Б под давлением)
                        float absMass = math.abs(massToMove);
                        MixFluids(ref mixtureA, in mixtureB, absMass);
                        mixtureB.TotalMass -= absMass;
                    }

                    MixtureLookup[link.NodeA] = mixtureA;
                    MixtureLookup[link.NodeB] = mixtureB;
                }
            }

            // Вспомогательный метод для просчета долей при смешивании
            private void MixFluids(ref FluidMixture target, in FluidMixture source, float addedMass)
            {
                float newTotalMass = target.TotalMass + addedMass;
                if (newTotalMass <= 0.0001f) return; // Защита от деления на ноль

                // Пересчет долей по формуле средневзвешенного (масса компонента = ОбщаяМасса * Доля)
                target.FractionGas = (target.FractionGas * target.TotalMass + source.FractionGas * addedMass) / newTotalMass;
                target.FractionLightNaphtha = (target.FractionLightNaphtha * target.TotalMass + source.FractionLightNaphtha * addedMass) / newTotalMass;
                target.FractionHeavyNaphtha = (target.FractionHeavyNaphtha * target.TotalMass + source.FractionHeavyNaphtha * addedMass) / newTotalMass;
                target.FractionKerosene = (target.FractionKerosene * target.TotalMass + source.FractionKerosene * addedMass) / newTotalMass;
                target.FractionLightDiesel = (target.FractionLightDiesel * target.TotalMass + source.FractionLightDiesel * addedMass) / newTotalMass;
                target.FractionHeavyDiesel = (target.FractionHeavyDiesel * target.TotalMass + source.FractionHeavyDiesel * addedMass) / newTotalMass;
                target.FractionMazut = (target.FractionMazut * target.TotalMass + source.FractionMazut * addedMass) / newTotalMass;
                target.FractionWater = (target.FractionWater * target.TotalMass + source.FractionWater * addedMass) / newTotalMass;

                // Смешивание температур (упрощенное, на базе массы)
                target.Temperature = (target.Temperature * target.TotalMass + source.Temperature * addedMass) / newTotalMass;
                
                // Обновляем итоговую массу принимающего узла
                target.TotalMass = newTotalMass;
            }
        }
    }
}