using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using SimBridge.Core.Time;

namespace SimOil
{
    // Гарантируем, что система существует и обновляется ТОЛЬКО в серверном мире
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct FluidFlowSystem : ISystem
    {
        private ComponentLookup<FluidMixture> _mixtureLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationTimeComponent>();
            state.RequireForUpdate<SimulationOilControl>();
            
            // Инициализируем lookup (доступ к компонентам по Entity)
            _mixtureLookup = state.GetComponentLookup<FluidMixture>(isReadOnly: false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeData = SystemAPI.GetSingleton<SimulationTimeComponent>();
            var oilControl = SystemAPI.GetSingleton<SimulationOilControl>();
            
            // Если симуляция выключена или на паузе — выходим
            if (!oilControl.IsRunning || timeData.TimeScale <= 0f) return;

            // Обновляем указатели на память для текущего кадра
            _mixtureLookup.Update(ref state);

            var flowJob = new CalculateFlowJob
            {
                MixtureLookup = _mixtureLookup,
                FixedStep = timeData.FixedStep
            };

            // ИСПРАВЛЕНИЕ: Вызываем Schedule без аргументов. 
            // Unity сама соберет запрос для всех сущностей с компонентом FluidLink.
            // Schedule запускает джобу в фоновом потоке, но в один поток (не Parallel), 
            // что защищает нас от состояния гонки при записи в общие узлы MixtureLookup.
            state.Dependency = flowJob.Schedule(state.Dependency);
        }

        [BurstCompile]
        private partial struct CalculateFlowJob : IJobEntity
        {
            public ComponentLookup<FluidMixture> MixtureLookup;
            public float FixedStep;

            // Генератор кода сам найдет все сущности с RefRW<FluidLink>
            public void Execute(ref FluidLink link)
            {
                if (!MixtureLookup.HasComponent(link.NodeA) || !MixtureLookup.HasComponent(link.NodeB))
                    return;

                var mixtureA = MixtureLookup[link.NodeA];
                var mixtureB = MixtureLookup[link.NodeB];

                float deltaP = mixtureA.Pressure - mixtureB.Pressure;
                float flowDirection = math.sign(deltaP);

                // Базовый расчет расхода: Q = Площадь * корень(dP)
                float targetFlowRate = math.sqrt(math.abs(deltaP)) * link.CrossSectionArea * flowDirection;
                link.CurrentFlowRateMass = targetFlowRate;

                float massToMove = targetFlowRate * FixedStep;

                // Защита от ухода массы в минус
                if (massToMove > 0 && massToMove > mixtureA.TotalMass) massToMove = mixtureA.TotalMass;
                if (massToMove < 0 && math.abs(massToMove) > mixtureB.TotalMass) massToMove = -mixtureB.TotalMass;

                if (math.abs(massToMove) > 0.0001f)
                {
                    // Временная заглушка изменения массы (для проверки разницы давлений)
                    mixtureA.TotalMass -= massToMove;
                    mixtureB.TotalMass += massToMove;

                    MixtureLookup[link.NodeA] = mixtureA;
                    MixtureLookup[link.NodeB] = mixtureB;
                }
            }
        }
    }
}