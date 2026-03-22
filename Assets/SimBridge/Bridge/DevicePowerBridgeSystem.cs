using SimVent.Components;
using Unity.Burst;
using Unity.Entities;

namespace SimElectric.Bridge
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CircuitSolverSystem))] // Сначала считаем ток, потом передаем команду
    [BurstCompile]
    public partial struct DevicePowerBridgeSystem : ISystem
    {
        private ComponentLookup<ElectricalNode> nodeLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationControl>();
            nodeLookup = state.GetComponentLookup<ElectricalNode>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var control = SystemAPI.GetSingleton<SimulationControl>();
            if (!control.IsRunning && !control.StepNextFrame) return;

            nodeLookup.Update(ref state);

            var job = new UpdateDevicePowerJob
            {
                NodeLookup = nodeLookup
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct UpdateDevicePowerJob : IJobEntity
        {
            [Unity.Collections.ReadOnly]
            public ComponentLookup<ElectricalNode> NodeLookup;

            // Магия DOTS: Джоб автоматически найдет все сущности, 
            // у которых есть И интерфейс питания, И вентилятор!
            private void Execute(ref ElectricalDeviceInterface device, ref FanComponent fan)
            {
                if (NodeLookup.HasComponent(device.PowerTerminal))
                {
                    var node = NodeLookup[device.PowerTerminal];
                    
                    // Питание есть, если напряжение достаточное И есть путь к земле
                    device.IsPowered = (node.CurrentVoltage >= device.ActivationVoltage) && node.CurrentHasGround;
                    
                    // Передаем команду в механику вентилятора!
                    fan.RunCommand = device.IsPowered;
                }
            }
        }
    }
}