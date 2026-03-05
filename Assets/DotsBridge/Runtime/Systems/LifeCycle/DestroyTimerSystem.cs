using Unity.Burst;
using Unity.Entities;

namespace DotsBridge.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
    public partial struct DestroyTimerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DestroyTimer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new DestroyTimerJob
            {
                DeltaTime = dt,
                Ecb = ecb
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct DestroyTimerJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute(Entity entity, [ChunkIndexInQuery] int sortKey, ref DestroyTimer timer)
        {
            timer.Value -= DeltaTime;

            if (timer.Value <= 0f)
            {
                // 1. ВКЛЮЧАЕМ предсмертный хрип на этот кадр
                Ecb.SetComponentEnabled<DeathEvent>(sortKey, entity, true);

                // 2. Снимаем таймер, он свою работу выполнил
                Ecb.RemoveComponent<DestroyTimer>(sortKey, entity);
            }
        }
    }
}