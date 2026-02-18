using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace DotsBridge.Movement
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct PhysicsMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (velocity, dir, speed) in
                     SystemAPI.Query<RefRW<PhysicsVelocity>, RefRO<MoveDirection>, RefRO<MoveSpeed>>()
                     .WithAll<MoveWithPhysicsTag>())
            {
                float y = velocity.ValueRO.Linear.y;
                float3 move = dir.ValueRO.Value * speed.ValueRO.Value;
                move.y = y;

                velocity.ValueRW.Linear = move;
            }

            foreach (var (velocity, transform, target, speed, stopDist, entity) in
                     SystemAPI.Query<RefRW<PhysicsVelocity>, RefRO<LocalTransform>, RefRO<MoveTarget>, RefRO<MoveSpeed>, RefRO<StopDistance>>()
                     .WithAll<MoveWithPhysicsTag>()
                     .WithEntityAccess())
            {
                float3 toTarget = target.ValueRO.Value - transform.ValueRO.Position;
                toTarget.y = 0;

                if (math.lengthsq(toTarget) <= stopDist.ValueRO.Value * stopDist.ValueRO.Value)
                {
                    // Пришли -> Стоп
                    var vel = velocity.ValueRO.Linear;
                    vel.x = 0;
                    vel.z = 0;
                    velocity.ValueRW.Linear = vel;

                    state.EntityManager.SetComponentEnabled<MoveWithPhysicsTag>(entity, false);
                }
                else
                {
                    float3 dir = math.normalize(toTarget);
                    float y = velocity.ValueRO.Linear.y;
                    float3 move = dir * speed.ValueRO.Value;
                    move.y = y;
                    velocity.ValueRW.Linear = move;
                }
            }
        }
    }
}