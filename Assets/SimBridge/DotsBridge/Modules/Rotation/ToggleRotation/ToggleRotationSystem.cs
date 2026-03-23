using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsBridge.Systems
{
    [UpdateBefore(typeof(TransformSystemGroup))]
    [BurstCompile]
    public partial struct ToggleRotationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Получаем Lookup с правом на запись (isReadOnly: false)
            var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(false);
            
            // Запускаем Job через Schedule(), а не ScheduleParallel()
            state.Dependency = new RotationJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                TransformLookup = transformLookup
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        public partial struct RotationJob : IJobEntity
        {
            public float DeltaTime;
            
            // Контейнер без [ReadOnly], так как мы пишем в него
            public ComponentLookup<LocalTransform> TransformLookup;

            public void Execute(in ToggleRotation data)
            {
                if (data.IsOn && TransformLookup.HasComponent(data.Target))
                {
                    var lt = TransformLookup[data.Target];
                    
                    lt.Rotation = math.mul(
                        lt.Rotation, 
                        quaternion.AxisAngle(data.Axis, data.Speed * DeltaTime)
                    );
                    
                    TransformLookup[data.Target] = lt;
                }
            }
        }
    }
}