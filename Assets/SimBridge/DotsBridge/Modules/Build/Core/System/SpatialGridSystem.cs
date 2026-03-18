using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Build
{

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SpatialGridSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            var map = new NativeParallelMultiHashMap<int3, Entity>(10000, Allocator.Persistent);
            
            var singletonEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(singletonEntity, new SpatialGridData { Map = map });
        }

        public void OnDestroy(ref SystemState state)
        {
            // Обязательно освобождаем неуправляемую память при уничтожении мира
            if (SystemAPI.TryGetSingleton<SpatialGridData>(out var gridData))
            {
                if (gridData.Map.IsCreated)
                {
                    gridData.Map.Dispose();
                }
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<SpatialGridData>(out var gridData)) return;

            // 1. Очищаем карту перед новым кадром
            gridData.Map.Clear();

            // 2. Запускаем многопоточную задачу (Job) для заполнения
            var job = new BuildGridJob
            {
                GridMap = gridData.Map.AsParallelWriter()
            };

            // ScheduleParallel раскидает сущности по всем доступным ядрам процессора
            job.ScheduleParallel();
        }

        [BurstCompile]
        private partial struct BuildGridJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int3, Entity>.ParallelWriter GridMap;

            // Job автоматически найдет все сущности с компонентом GridPosition
            public void Execute(Entity entity, in GridPosition gridPos)
            {
                // Записываем сущность по её координате
                GridMap.Add(gridPos.Value, entity);
            }
        }
    }
}