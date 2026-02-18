using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsBridge.Movement
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
    public partial struct TransformMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // 1. По Направлению
            foreach (var (transform, dir, speed) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveDirection>, RefRO<MoveSpeed>>()
                     .WithAll<MoveWithTransformTag>())
            {
                transform.ValueRW.Position += dir.ValueRO.Value * speed.ValueRO.Value * dt;
            }

            // 2. К Точке
            foreach (var (transform, target, speed, stopDist, entity) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveTarget>, RefRO<MoveSpeed>, RefRO<StopDistance>>()
                     .WithAll<MoveWithTransformTag>()
                     .WithEntityAccess())
            {
                float3 toTarget = target.ValueRO.Value - transform.ValueRO.Position;
                float distSq = math.lengthsq(toTarget);

                if (distSq <= stopDist.ValueRO.Value * stopDist.ValueRO.Value)
                {
                    state.EntityManager.SetComponentEnabled<MoveWithTransformTag>(entity, false);
                }
                else
                {
                    float3 dir = math.normalize(toTarget);
                    transform.ValueRW.Position += dir * speed.ValueRO.Value * dt;
                }
            }
        }
    }
}