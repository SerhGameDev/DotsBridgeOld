using Unity.Burst;
using Unity.Entities;
using SimElectric;

namespace SimElectric.Interaction
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(CircuitSolverSystem))]
    [BurstCompile]
    public partial struct PushButtonLogicSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationControl>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var control = SystemAPI.GetSingleton<SimulationControl>();
            if (!control.IsRunning && !control.StepNextFrame) return;

            float deltaTime = SystemAPI.Time.DeltaTime;

            var job = new UpdateButtonJob { DeltaTime = deltaTime };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct UpdateButtonJob : IJobEntity
        {
            public float DeltaTime;

            private void Execute(ref PushButton button, ref Wire wire)
            {
                if (button.IsPressed)
                {
                    // Если кнопка с пружиной, запускаем обратный отсчет
                    if (button.IsMomentary)
                    {
                        button.CurrentTimer -= DeltaTime;
                        if (button.CurrentTimer <= 0f)
                        {
                            button.IsPressed = false; // Пружина откинула кнопку
                        }
                    }
                }

                // Физическое состояние провода строго равно состоянию кнопки
                wire.IsConducting = button.IsPressed;
            }
        }
    }
}