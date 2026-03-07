using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DotsBridge.Modules.Movement
{
    // Обязательно указываем, что система работает в мультиплеере!
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct MovementTransformSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new MoveJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref LocalTransform transform, in MoveTransformSpeed speed, in MoveTransformDirection direction, in IsTransformMoving tag)
        {
            transform.Position += direction.Value * speed.Value * DeltaTime;
        }
    }
}