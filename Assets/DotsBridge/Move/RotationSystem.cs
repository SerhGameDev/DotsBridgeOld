using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsBridge.Movement
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformMovementSystem))] 
    [BurstCompile]
    public partial struct RotationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            foreach (var (transform, dir, rotSpeed) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveDirection>, RefRO<RotationSpeed>>()
                     .WithAll<RotateToMovementTag>()) 
            {
                if (math.lengthsq(dir.ValueRO.Value) > 0.001f)
                {
                    quaternion targetRot = quaternion.LookRotation(dir.ValueRO.Value, math.up());
                    transform.ValueRW.Rotation = math.slerp(transform.ValueRW.Rotation, targetRot, rotSpeed.ValueRO.Value * dt);
                }
            }

            foreach (var (transform, target, rotSpeed) in
                    SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveTarget>, RefRO<RotationSpeed>>()
                    .WithAll<RotateToMovementTag>())
            {
                float3 dir = target.ValueRO.Value - transform.ValueRO.Position;
                dir.y = 0;

                if (math.lengthsq(dir) > 0.001f)
                {
                    quaternion targetRot = quaternion.LookRotation(math.normalize(dir), math.up());
                    transform.ValueRW.Rotation = math.slerp(transform.ValueRW.Rotation, targetRot, rotSpeed.ValueRO.Value * dt);
                }
            }
        }
    }
}