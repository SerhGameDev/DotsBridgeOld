using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsBridge.Modules.Rotation
{
    [WorldSystemFilter(WorldSystemFilterFlags.All)]
    public partial struct RotationTransformSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new RotateJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct RotateJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref LocalTransform transform, in RotateTransformSpeed speed, in RotateTransformAxis axis, in IsTransformRotating tag)
        {
            // Железобетонный предохранитель от нулей
            if (math.lengthsq(axis.Value) < 0.0001f)
                return;

            float angle = speed.Value * DeltaTime;
            quaternion deltaRotation = quaternion.AxisAngle(axis.Value, angle);

            if (axis.IsLocal)
            {
                // Локальное вращение (вокруг собственных осей объекта)
                transform = transform.Rotate(deltaRotation);
            }
            else
            {
                // Глобальное вращение (вокруг мировых осей сцены)
                // Меняем порядок умножения: Сначала дельта, потом текущее вращение
                transform.Rotation = math.mul(deltaRotation, transform.Rotation);
            }
        }
    }
}