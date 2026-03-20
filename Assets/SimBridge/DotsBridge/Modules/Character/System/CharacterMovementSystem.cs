using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Collections; // Важно: добавлено для NativeList
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace DotsBridge.Character
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CharacterLookSystem))]
    public partial class CharacterMovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorld))
                return;

            float deltaTime = SystemAPI.Time.DeltaTime;

            // ВАЖНО: Добавлено .WithEntityAccess(), чтобы получить текущую сущность
            foreach (var (transform, input, settings, velocity, entity) in SystemAPI.Query<
                         RefRW<LocalTransform>, 
                         RefRO<CharacterControlInput>, 
                         RefRO<CharacterSettings>, 
                         RefRW<CharacterVelocity>>()
                         .WithEntityAccess()
                         .WithAll<ActiveCharacterTag>())
            {
                float3 currentPos = transform.ValueRO.Position;

                // --- 1. ГОРИЗОНТАЛЬНОЕ ДВИЖЕНИЕ ---
                float3 forward = math.normalize(math.mul(transform.ValueRO.Rotation, math.forward()));
                float3 right = math.normalize(math.mul(transform.ValueRO.Rotation, math.right()));
                forward.y = 0; right.y = 0;

                float3 desiredMoveDir = (forward * input.ValueRO.MoveInput.y + right * input.ValueRO.MoveInput.x);

                if (math.lengthsq(desiredMoveDir) > 0.01f)
                {
                    desiredMoveDir = math.normalize(desiredMoveDir);
                    float moveDistance = settings.ValueRO.MoveSpeed * deltaTime;

                    float3 rayStart = currentPos + new float3(0, settings.ValueRO.StepHeight + 0.1f, 0);
                    float3 rayEnd = rayStart + desiredMoveDir * (settings.ValueRO.CharacterRadius + moveDistance);

                    var wallInput = new RaycastInput { Start = rayStart, End = rayEnd, Filter = CollisionFilter.Default };

                    // Заменили стандартный CastRay на наш безопасный метод
                    if (CastRayIgnoreEntity(ref physicsWorld, wallInput, entity, out var wallHit))
                    {
                        float dot = math.dot(desiredMoveDir, wallHit.SurfaceNormal);
                        if (dot < 0) 
                        {
                            desiredMoveDir = desiredMoveDir - wallHit.SurfaceNormal * dot;
                            if (math.lengthsq(desiredMoveDir) > 0.001f) desiredMoveDir = math.normalize(desiredMoveDir);
                            else desiredMoveDir = float3.zero;
                        }
                    }

                    currentPos += desiredMoveDir * moveDistance;
                }

                // --- 2. ВЕРТИКАЛЬНОЕ ДВИЖЕНИЕ (Гравитация и Ступени) ---
                
                // ПРОБЛЕМНАЯ ЗОНА №2: Убедись, что settings.Gravity = -15 (с минусом!)
                velocity.ValueRW.Value.y += settings.ValueRO.Gravity * deltaTime;
                float gravityStepY = velocity.ValueRO.Value.y * deltaTime;
                
                float castHeight = settings.ValueRO.StepHeight + 0.2f;
                float3 groundRayStart = currentPos + new float3(0, castHeight, 0);
                float3 groundRayEnd = groundRayStart + new float3(0, -castHeight - 0.1f + math.min(0, gravityStepY), 0);

                var groundInput = new RaycastInput { Start = groundRayStart, End = groundRayEnd, Filter = CollisionFilter.Default };

                // Заменили стандартный CastRay на наш безопасный метод
                bool hitGround = CastRayIgnoreEntity(ref physicsWorld, groundInput, entity, out var groundHit);

                if (hitGround)
                {
                    velocity.ValueRW.Value.y = 0;
                    velocity.ValueRW.IsGrounded = true;
                    
                    float targetY = groundHit.Position.y;
                    currentPos.y = math.lerp(currentPos.y, targetY, 15f * deltaTime);
                }
                else
                {
                    velocity.ValueRW.IsGrounded = false;
                    currentPos.y += gravityStepY;
                }

                transform.ValueRW.Position = currentPos;
            }
        }

        /// <summary>
        /// Вспомогательный метод: собирает все попадания луча и возвращает ближайшее, 
        /// которое НЕ является самим персонажем. Идеально для защиты от самоколлизий.
        /// </summary>
        private bool CastRayIgnoreEntity(ref PhysicsWorldSingleton physicsWorld, RaycastInput input, Entity ignoreEntity, out RaycastHit closestValidHit)
        {
            closestValidHit = default;
            bool found = false;
            float closestFraction = float.MaxValue;

            // Выделяем временную память под массив попаданий
            var hits = new NativeList<RaycastHit>(Allocator.Temp);
            
            if (physicsWorld.CollisionWorld.CastRay(input, ref hits))
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    var hit = hits[i];
                    
                    // Главная проверка: игнорируем собственный Entity
                    if (hit.Entity != ignoreEntity && hit.Fraction < closestFraction)
                    {
                        closestFraction = hit.Fraction;
                        closestValidHit = hit;
                        found = true;
                    }
                }
            }
            
            // DOTS требует ручной очистки Native-коллекций!
            hits.Dispose(); 
            
            return found;
        }
    }
}