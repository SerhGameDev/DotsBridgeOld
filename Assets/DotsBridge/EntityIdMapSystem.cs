using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace DotsBridge
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct EntityIdMapSystem : ISystem
    {
        private EntityQuery _query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _query = state.GetEntityQuery(ComponentType.ReadOnly<ID>());
            state.RequireForUpdate(_query);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Dots.MapDependency.Complete();

            if (SystemAPI.HasSingleton<EntityIdMap>())
            {
                var map = SystemAPI.GetSingleton<EntityIdMap>().Map;
                if (map.IsCreated) map.Dispose();
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<EntityIdMap>())
            {
                int initialCap = _query.CalculateEntityCount();
                var map = new NativeParallelMultiHashMap<int, Entity>(initialCap > 0 ? initialCap : 128, Allocator.Persistent);
                state.EntityManager.CreateSingleton(new EntityIdMap { Map = map });
            }

            ref var mapRef = ref SystemAPI.GetSingletonRW<EntityIdMap>().ValueRW;
            mapRef.Map.Clear();

            int count = _query.CalculateEntityCount();
            if (count > mapRef.Map.Capacity) mapRef.Map.Capacity = count * 2;

            var job = new RebuildMapJob
            {
                MapWriter = mapRef.Map.AsParallelWriter()
            };

            // Запускаем Job и получаем Handle
            JobHandle handle = job.ScheduleParallel(_query, state.Dependency);

            // Сохраняем Handle в систему...
            state.Dependency = handle;

            // ...И передаем его в наш статический класс!
            Dots.MapDependency = handle;
        }
    }

    // RebuildMapJob тот же самый...
    [BurstCompile]
    public partial struct RebuildMapJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int, Entity>.ParallelWriter MapWriter;
        private void Execute(Entity entity, in ID id)
        {
            MapWriter.Add(id.Value, entity);
        }
    }
}