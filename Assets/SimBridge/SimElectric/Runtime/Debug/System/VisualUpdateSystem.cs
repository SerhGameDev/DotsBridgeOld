using SimElectric;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

namespace SimElectric
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct VisualUpdateSystem : ISystem
    {
        private ComponentLookup<ElectricalNode> nodeLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Система работает ТОЛЬКО если существует этот тег

            nodeLookup = state.GetComponentLookup<ElectricalNode>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            nodeLookup.Update(ref state);

            // 1. Обновляем цвета проводов
            var wireVisualJob = new WireVisualJob
            {
                NodeLookup = nodeLookup
            };
            state.Dependency = wireVisualJob.ScheduleParallel(state.Dependency);

            // 2. Обновляем цвета лампочек
            var lightbulbJob = new LightbulbVisualJob();
            state.Dependency = lightbulbJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct WireVisualJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<ElectricalNode> NodeLookup;

            // Пишем прямо в URPMaterialPropertyBaseColor, чтобы изменить цвет меша
            private void Execute(in Wire wire, in WireVisuals visuals, ref URPMaterialPropertyBaseColor materialColor)
            {
                bool hasVoltage = false;

                // Провод под напряжением, если хотя бы на одном из его концов есть потенциал
                if (NodeLookup.HasComponent(wire.NodeA) && NodeLookup.HasComponent(wire.NodeB))
                {
                    float voltageA = NodeLookup[wire.NodeA].CurrentVoltage;
                    float voltageB = NodeLookup[wire.NodeB].CurrentVoltage;

                    if (voltageA > 0.1f || voltageB > 0.1f)
                    {
                        hasVoltage = true;
                    }
                }

                // Меняем цвет в зависимости от наличия напряжения
                materialColor.Value = hasVoltage ? visuals.ActiveColor : visuals.InactiveColor;
            }
        }

        [BurstCompile]
        public partial struct LightbulbVisualJob : IJobEntity
        {
            private void Execute(in ElectricalNode node, in Lightbulb lightbulb, ref URPMaterialPropertyBaseColor materialColor)
            {
                // Лампочка горит, если есть напряжение И есть путь к земле
                bool isOn = (node.CurrentVoltage >= lightbulb.ThresholdVoltage) && node.CurrentHasGround;
                materialColor.Value = isOn ? lightbulb.OnColor : lightbulb.OffColor;
            }
        }
    }
}