using Unity.Burst;
using Unity.Entities;
using SimOil.Network;
using Unity.Collections;

namespace SimOil.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FluidFlowSystem))] // Пакуем строго после расчетов потока
    [BurstCompile]
    public partial struct NetSyncPackingSystem : ISystem
    {
        private ComponentLookup<FluidMixture> _mixtureLookup;
        private ComponentLookup<FluidLink> _linkLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _mixtureLookup = state.GetComponentLookup<FluidMixture>(isReadOnly: true);
            _linkLookup = state.GetComponentLookup<FluidLink>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _mixtureLookup.Update(ref state);
            _linkLookup.Update(ref state);

            // Джоба для упаковки узлов (Резервуаров)
            var mixturePackJob = new PackMixtureJob
            {
                MixtureLookup = _mixtureLookup
            };
            state.Dependency = mixturePackJob.ScheduleParallel(state.Dependency);

            // Джоба для упаковки связей (Труб)
            var linkPackJob = new PackLinkJob
            {
                LinkLookup = _linkLookup
            };
            state.Dependency = linkPackJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct PackMixtureJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<FluidMixture> MixtureLookup;

            public void Execute(ref NetSync_FluidMixture syncData, in SimTargetLink link)
            {
                if (MixtureLookup.HasComponent(link.TargetEntity))
                {
                    var mix = MixtureLookup[link.TargetEntity];
                    
                    syncData.TotalMass = mix.TotalMass;
                    syncData.Pressure = mix.Pressure;
                    syncData.Temperature = mix.Temperature;

                    // Определение преобладающей фракции для цвета (упрощенная логика)
                    syncData.DominantFraction = GetDominantFraction(in mix);
                }
            }

            private FractionType GetDominantFraction(in FluidMixture mix)
            {
                // Находим фракцию с максимальной долей для передачи на клиент (для цвета трубы)
                FractionType dom = FractionType.Water;
                float max = mix.FractionWater;

                if (mix.FractionMazut > max) { max = mix.FractionMazut; dom = FractionType.Mazut; }
                if (mix.FractionLightNaphtha > max) { max = mix.FractionLightNaphtha; dom = FractionType.LightNaphtha; }
                if (mix.FractionKerosene > max) { max = mix.FractionKerosene; dom = FractionType.Kerosene; }
                if (mix.FractionGas > max) { dom = FractionType.Gas; } // Газ всегда перекрывает визуал, если его много

                return dom;
            }
        }

        [BurstCompile]
        private partial struct PackLinkJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<FluidLink> LinkLookup;

            public void Execute(ref NetSync_FluidLink syncData, in SimTargetLink link)
            {
                if (LinkLookup.HasComponent(link.TargetEntity))
                {
                    syncData.FlowRate = LinkLookup[link.TargetEntity].CurrentFlowRateMass;
                }
            }
        }
    }
}