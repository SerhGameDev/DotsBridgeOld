using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsBridge.Systems
{
    [BurstCompile]
    public partial struct ToggleRotationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            foreach (var (trans, data) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ToggleRotation>>())
            {
                if (data.ValueRO.IsOn)
                {
                    trans.ValueRW.Rotation = math.mul(
                        trans.ValueRW.Rotation,
                        quaternion.AxisAngle(data.ValueRO.Axis, data.ValueRO.Speed * dt)
                    );
                }
            }
        }
    }
}