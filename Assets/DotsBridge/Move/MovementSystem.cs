using DotsBridge.Movement;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics; 
using Unity.Transforms;

namespace DotsBridge
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    [BurstCompile]
    public partial struct MovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            foreach (var (transform, agent, target, velocity, entity) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveAgent>, RefRO<MoveTarget>, RefRW<PhysicsVelocity>>()
                     .WithAll<IsMovingTag>()
                     .WithEntityAccess())
            {
                float3 currentPos = transform.ValueRO.Position;
                float3 destPos = target.ValueRO.Value;
                float3 toDest = destPos - currentPos;
                float dist = math.length(toDest);

                if (dist <= agent.ValueRO.StoppingDistance)
                {
                    velocity.ValueRW.Linear = float3.zero;
                    velocity.ValueRW.Angular = float3.zero;

                    state.EntityManager.SetComponentEnabled<IsMovingTag>(entity, false);
                    continue;
                }

                float3 dir = math.normalize(toDest);

                velocity.ValueRW.Linear = dir * agent.ValueRO.Speed;

                ApplyRotation(ref transform.ValueRW, dir, agent.ValueRO.RotationSpeed, dt);
            }

            foreach (var (transform, agent, direction, entity) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveAgent>, RefRO<MoveDirection>>()
                     .WithAll<IsMovingTag>()
                     .WithEntityAccess())
            {
                float3 dir = direction.ValueRO.Value;

                if (math.lengthsq(dir) < 0.001f)
                {
                    if (SystemAPI.HasComponent<PhysicsVelocity>(entity))
                    {
                        var vel = SystemAPI.GetComponentRW<PhysicsVelocity>(entity);
                        vel.ValueRW.Linear = float3.zero;
                    }
                    state.EntityManager.SetComponentEnabled<IsMovingTag>(entity, false);
                    continue;
                }

                if (SystemAPI.HasComponent<PhysicsVelocity>(entity))
                {
                    var vel = SystemAPI.GetComponentRW<PhysicsVelocity>(entity);
                    float currentY = vel.ValueRO.Linear.y;
                    float3 moveVel = dir * agent.ValueRO.Speed;
                    moveVel.y = currentY;
                    vel.ValueRW.Linear = moveVel;
                }
                else
                {
                    transform.ValueRW.Position += dir * agent.ValueRO.Speed * dt;
                }

                ApplyRotation(ref transform.ValueRW, dir, agent.ValueRO.RotationSpeed, dt);
            }
        }

        // Хелпер для плавного поворота
        private void ApplyRotation(ref LocalTransform transform, float3 dir, float rotSpeed, float dt)
        {
            if (math.lengthsq(dir) < 0.001f) return;

            // Считаем целевой поворот (смотрит туда, куда идет)
            quaternion targetRot = quaternion.LookRotation(dir, math.up()); // math.up() для 3D, для 2D используйте math.forward()

            // Интерполируем (Slerp)
            transform.Rotation = math.slerp(transform.Rotation, targetRot, rotSpeed * dt);
        }
    }
}