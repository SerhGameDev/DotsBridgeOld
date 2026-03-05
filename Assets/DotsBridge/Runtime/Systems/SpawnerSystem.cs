using DotsBridge.Spawning;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace DotsBridge.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct SpawnerSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _transformLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpawnRequest>();
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Получаем синглтон ECB системы
            var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();

            // СОЗДАЕМ ПАРАЛЛЕЛЬНЫЙ WRITER для многопоточной записи
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            // Обновляем Lookup перед джобой
            _transformLookup.Update(ref state);

            // Запускаем джобу ПАРАЛЛЕЛЬНО на всех ядрах
            new SpawnerJob
            {
                Ecb = ecb,
                TransformLookup = _transformLookup
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct SpawnerJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;

        private void Execute(Entity entity, [ChunkIndexInQuery] int sortKey, in SpawnRequest req)
        {
            float finalScale = req.Scale;

            if (!req.OverrideScale && req.Prefab != Entity.Null && TransformLookup.HasComponent(req.Prefab))
            {
                finalScale = TransformLookup[req.Prefab].Scale;
            }

            var spawnTransform = LocalTransform.FromPositionRotationScale(req.Position, req.Rotation, finalScale);

            for (int i = 0; i < req.Count; i++)
            {
                Entity newEntity;

                if (req.Prefab != Entity.Null)
                {
                    newEntity = Ecb.Instantiate(sortKey, req.Prefab);
                }
                else
                {
                    newEntity = Ecb.CreateEntity(sortKey);
                }

                Ecb.AddComponent(sortKey, newEntity, spawnTransform);

                if (req.ID != 0)
                {
                    Ecb.AddComponent(sortKey, newEntity, new EntityIdComponent { Hash = req.ID });
                }
            }

            Ecb.DestroyEntity(sortKey, entity);
        }
    }
}