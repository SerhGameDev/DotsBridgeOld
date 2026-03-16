using SimElectric;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace SimElectric
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CircuitSolverSystem))]
    [BurstCompile]
    public partial struct RelayLogicSystem : ISystem
    {
        private ComponentLookup<ElectricalNode> nodeLookup;
        private ComponentLookup<RelayCoil> coilLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationControl>();
            nodeLookup = state.GetComponentLookup<ElectricalNode>(isReadOnly: true);
            coilLookup = state.GetComponentLookup<RelayCoil>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var control = SystemAPI.GetSingleton<SimulationControl>();
            if (!control.IsRunning && !control.StepNextFrame) return;

            nodeLookup.Update(ref state);
            coilLookup.Update(ref state);

            // 1. Оцениваем состояние всех катушек (Job 1)
            var evaluateCoilsJob = new EvaluateCoilsJob
            {
                NodeLookup = nodeLookup
            };
            state.Dependency = evaluateCoilsJob.ScheduleParallel(state.Dependency);

            // 2. Применяем состояние катушек к физическим контактам-проводам (Job 2)
            var updateContactsJob = new UpdateContactsJob
            {
                CoilLookup = coilLookup
            };
            // Этот Job ждет завершения первого
            state.Dependency = updateContactsJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct EvaluateCoilsJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<ElectricalNode> NodeLookup;

            private void Execute(ref RelayCoil coil)
            {
                if (NodeLookup.HasComponent(coil.NodeA1) && NodeLookup.HasComponent(coil.NodeA2))
                {
                    var nodeA1 = NodeLookup[coil.NodeA1];
                    var nodeA2 = NodeLookup[coil.NodeA2];

                    float deltaU = math.abs(nodeA1.CurrentVoltage - nodeA2.CurrentVoltage);

                    // ПРОВЕРКА ЦЕПИ: Катушка сработает, только если есть напряжение И хотя бы один пин соединен с землей
                    bool hasGroundPath = nodeA1.CurrentHasGround || nodeA2.CurrentHasGround;

                    coil.IsEnergized = (deltaU >= coil.EnergizeThreshold) && hasGroundPath;
                }
            }
        }
        [BurstCompile]
        public partial struct UpdateContactsJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<RelayCoil> CoilLookup;

            // Джоб выполняется только для сущностей, у которых есть И Wire, И RelayContact
            private void Execute(ref Wire wire, in RelayContact contact)
            {
                if (CoilLookup.HasComponent(contact.TargetCoil))
                {
                    bool isCoilEnergized = CoilLookup[contact.TargetCoil].IsEnergized;

                    // Меняем проводимость провода в зависимости от типа контакта
                    if (contact.Type == ContactType.NormallyOpen)
                    {
                        wire.IsConducting = isCoilEnergized; // NO: проводит, когда включено
                    }
                    else if (contact.Type == ContactType.NormallyClosed)
                    {
                        wire.IsConducting = !isCoilEnergized; // NC: проводит, когда выключено
                    }
                }
            }
        }
    }
}