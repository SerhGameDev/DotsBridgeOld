using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs; // Убедитесь, что это подключено

namespace DotsBridge
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct IdMapSystem : ISystem
    {
        public NativeParallelMultiHashMap<int, Entity> EntityMap;
        
        // Добавляем публичный хэндл для синхронизации извне
        public JobHandle WriteHandle; 
        
        private EntityQuery _query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _query = state.GetEntityQuery(ComponentType.ReadOnly<BridgeIdentity>());
            EntityMap = new NativeParallelMultiHashMap<int, Entity>(256, Allocator.Persistent);
            state.RequireForUpdate(_query);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (EntityMap.IsCreated) EntityMap.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            int entityCount = _query.CalculateEntityCount();
            if (EntityMap.Capacity < entityCount)
            {
                EntityMap.Capacity = entityCount * 2; 
            }

            EntityMap.Clear();

            var job = new FillMapJob
            {
                MapWriter = EntityMap.AsParallelWriter()
            };

            // 1. Планируем джоб
            WriteHandle = job.ScheduleParallel(state.Dependency);
            
            // 2. Отдаем хэндл обратно системе ECS
            state.Dependency = WriteHandle; 
            
            // Заметьте: мы больше не вызываем state.Dependency.Complete() здесь!
        }

        [BurstCompile]
        public partial struct FillMapJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int, Entity>.ParallelWriter MapWriter;

            private void Execute(Entity entity, in BridgeIdentity idComponent)
            {
                MapWriter.Add(idComponent.Hash, entity);
            }
        }
    }
}