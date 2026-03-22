using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsBridge.Interaction
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class HingeAnimationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = SystemAPI.Time.DeltaTime;

            foreach (var (transform, hinge) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<HingeState>>())
            {
                // Если разница между текущим и целевым состоянием минимальна — пропускаем вычисления
                if (math.abs(hinge.ValueRO.CurrentState - hinge.ValueRO.TargetState) < 0.001f)
                    continue;

                float diff = hinge.ValueRO.TargetState - hinge.ValueRO.CurrentState;
                // Определяем направление (открытие или закрытие) и шаг
                float step = math.sign(diff) * hinge.ValueRO.Speed * dt;

                // Защита от перелета (overshoot)
                if (math.abs(diff) <= math.abs(step))
                {
                    hinge.ValueRW.CurrentState = hinge.ValueRO.TargetState;
                }
                else
                {
                    hinge.ValueRW.CurrentState += step;
                }

                // Плавно интерполируем поворот
                transform.ValueRW.Rotation = math.slerp(
                    hinge.ValueRO.ClosedRotation, 
                    hinge.ValueRO.OpenRotation, 
                    hinge.ValueRO.CurrentState
                );
            }
        }
    }
}