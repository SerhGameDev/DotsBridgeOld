using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics; // Добавляем физику, чтобы сбросить инерцию
using DotsBridge.Spawning;

namespace DotsBridge.Systems
{
    // 1. МЕНЯЕМ ГРУППУ НА INITIALIZATION
    // Это гарантирует, что объект появится и встанет на место ДО того, как Unity начнет рисовать кадр.
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct SpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // 2. ИСПОЛЬЗУЕМ СООТВЕТСТВУЮЩИЙ ECB (EndInitialization)
            // Команды выполнятся сразу после инициализации, перед физикой.
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

                // --- ПОДГОТОВКА ДАННЫХ ---
                float prefabScale = 1f;
                bool hasPrefab = req.Prefab != Entity.Null;

                if (hasPrefab && SystemAPI.HasComponent<LocalTransform>(req.Prefab))
                {
                    prefabScale = SystemAPI.GetComponent<LocalTransform>(req.Prefab).Scale;
                }

                float finalScale = req.OverrideScale ? req.Scale : prefabScale;
                int countToSpawn = math.min(req.BatchSize, req.CountRemaining);

                // --- СПАУН ---
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

                    // 1. УСТАНОВКА ПОЗИЦИИ (AddComponent перезаписывает данные префаба)
                    var transform = LocalTransform.FromPositionRotationScale(req.Position, req.Rotation, finalScale);
                    ecb.AddComponent(newEntity, transform);

                    // 2. СБРОС ФИЗИКИ (Важно!)
                    // Если у префаба есть физика, он может "дернуться" по инерции.
                    // Мы добавляем команду сброса скорости в ноль.
                    // (Если физики нет - этот компонент просто добавится, это безопасно, но лучше проверить)
                    if (hasPrefab && SystemAPI.HasComponent<PhysicsVelocity>(req.Prefab))
                    {
                        ecb.AddComponent(newEntity, PhysicsVelocity.Zero);
                    }

                    // 3. УДАЛЕНИЕ ИНТЕРПОЛЯЦИИ (Супер важно для "полетов")
                    // Если на префабе есть TransformInterpolation, он будет сглаживать перемещение от 0,0,0.
                    // Лучше всего удалить этот компонент с префаба в редакторе.
                    // Но можно и кодом:
                    // ecb.RemoveComponent<TransformInterpolation>(newEntity); 

                    if (req.ID != 0)
                    {
                        ecb.AddComponent(newEntity, new ID { Value = req.ID });
                    }
                }

                // --- ЛОГИКА ЗАВЕРШЕНИЯ ---
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