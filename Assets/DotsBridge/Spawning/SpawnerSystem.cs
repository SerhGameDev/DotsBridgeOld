using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using DotsBridge.Spawning;

namespace DotsBridge.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct SpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (request, entity) in SystemAPI.Query<RefRW<SpawnRequest>>().WithEntityAccess())
            {
                ref var req = ref request.ValueRW;

                if (req.IsPaused) continue;

                req.Timer += dt;
                if (req.Timer < req.Interval) continue;

                req.Timer -= req.Interval;
                if (req.Interval <= 0) req.Timer = 0;

                float prefabScale = 1f;
                bool hasPrefab = req.Prefab != Entity.Null;

                if (hasPrefab && SystemAPI.HasComponent<LocalTransform>(req.Prefab))
                {
                    prefabScale = SystemAPI.GetComponent<LocalTransform>(req.Prefab).Scale;
                }

                float finalScale = req.OverrideScale ? req.Scale : prefabScale;
                int countToSpawn = math.min(req.BatchSize, req.CountRemaining);

                for (int i = 0; i < countToSpawn; i++)
                {
                    Entity newEntity;

                    if (hasPrefab)
                    {
                        newEntity = ecb.Instantiate(req.Prefab);
                    }
                    else
                    {
                        newEntity = ecb.CreateEntity();
                    }

                    var transform = LocalTransform.FromPositionRotationScale(req.Position, req.Rotation, finalScale);
                    ecb.AddComponent(newEntity, transform);

                    if (hasPrefab && SystemAPI.HasComponent<PhysicsVelocity>(req.Prefab))
                    {
                        ecb.AddComponent(newEntity, PhysicsVelocity.Zero);
                    }


                    if (req.ID != 0)
                    {
                        ecb.AddComponent(newEntity, new ID { Value = req.ID });
                    }
                }

                req.CountRemaining -= countToSpawn;

                if (req.CountRemaining <= 0)
                {
                    bool shouldLoop = false;

                    if (req.Loops == -1)
                    {
                        shouldLoop = true;
                    }
                    else if (req.Loops > 1)
                    {
                        req.Loops--;
                        shouldLoop = true;
                    }

                    if (shouldLoop)
                    {
                        req.CountRemaining = req.OriginalCount;
                    }
                    else
                    {
                        ecb.DestroyEntity(entity);
                    }
                }
            }
        }
    }
}