using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct IdMapSystem : ISystem
    {
        public NativeParallelMultiHashMap<int, Entity> EntityMap;

        public void OnCreate(ref SystemState state)
        {
            EntityMap = new NativeParallelMultiHashMap<int, Entity>(100, Allocator.Persistent);
            state.RequireForUpdate<EntityIdComponent>();
        }

        public void OnDestroy(ref SystemState state)
        {
            if (EntityMap.IsCreated) EntityMap.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityMap.Clear();

            var job = new FillMapJob
            {
                MapWriter = EntityMap.AsParallelWriter()
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct FillMapJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int, Entity>.ParallelWriter MapWriter;

            private void Execute(Entity entity, in EntityIdComponent idComponent)
            {
                MapWriter.Add(idComponent.Hash, entity);
            }
        }
    }
}