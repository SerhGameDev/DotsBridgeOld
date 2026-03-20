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
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (transform, hinge) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<HingeState>>())
            {
                // Если мы уже достигли цели — пропускаем
                if (math.abs(hinge.ValueRO.CurrentState - hinge.ValueRO.TargetState) < 0.001f)
                    continue;
                
                float diff = hinge.ValueRO.TargetState - hinge.ValueRO.CurrentState;
                float step = math.sign(diff) * hinge.ValueRO.Speed * deltaTime;

                // Защита от "перелета" значения (overshoot)
                if (math.abs(diff) <= math.abs(step))
                {
                    hinge.ValueRW.CurrentState = hinge.ValueRO.TargetState;
                }
                else
                {
                    hinge.ValueRW.CurrentState += step;
                }

                // Плавно интерполируем поворот между закрытым и открытым состоянием
                transform.ValueRW.Rotation = math.slerp(
                    hinge.ValueRO.ClosedRotation, 
                    hinge.ValueRO.OpenRotation, 
                    hinge.ValueRO.CurrentState
                );
            }
        }
    }
}