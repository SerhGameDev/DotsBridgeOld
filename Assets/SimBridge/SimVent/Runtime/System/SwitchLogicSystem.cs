using Unity.Burst;
using Unity.Entities;
using DotsBridge.Interaction;

namespace SimElectric
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(CircuitSolverSystem))] 
    [BurstCompile]
    public partial struct SwitchLogicSystem : ISystem
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

            var job = new UpdateSwitchJob();
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct UpdateSwitchJob : IJobEntity
        {
            private void Execute(ref Wire wire, ref SwitchComponent switchData, in HingeState hinge)
            {
                // Логика связи: 
                // HingeState.TargetState > 0.5 означает, что пользователь "кликнул" и рычаг пошел вверх
                bool isSwitchedOn = hinge.TargetState > 0.5f;

                // Обновляем состояние переключателя
                switchData.IsOn = isSwitchedOn;

                // Физически замыкаем или размыкаем провод
                wire.IsConducting = isSwitchedOn;
            }
        }
    }
}