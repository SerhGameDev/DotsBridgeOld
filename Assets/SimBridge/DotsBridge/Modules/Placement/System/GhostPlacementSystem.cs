using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace DotsBridge.Placement
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class GhostPlacementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (SystemAPI.QueryBuilder().WithAll<GhostTag, LocalTransform>().Build().IsEmpty)
                return;

            if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorld))
                return;

            var camera = Camera.main;
            if (camera == null) return;

            var ray = camera.ScreenPointToRay(Input.mousePosition);
            
            float3 hitPos = float3.zero;
            float3 normal = math.up();
            bool hasValidSurface = false;

            var input = new RaycastInput
            {
                Start = ray.origin,
                End = (float3)ray.origin + (float3)ray.direction * 100f,
                Filter = CollisionFilter.Default
            };

            // 1. ИДЕАЛЬНЫЙ РЕЙКАСТ (Сбор всех попаданий)
            // Создаем временный список для хранения всех объектов, пробитых лучом
            var hits = new NativeList<RaycastHit>(Allocator.Temp);

            if (physicsWorld.CollisionWorld.CastRay(input, ref hits))
            {
                float closestFraction = float.MaxValue;

                // Перебираем все пробитые объекты
                for (int i = 0; i < hits.Length; i++)
                {
                    var hit = hits[i];

                    // Ищем объект с SurfaceTag, который находится ближе всего к камере
                    if (SystemAPI.HasComponent<SurfaceTag>(hit.Entity) && hit.Fraction < closestFraction)
                    {
                        closestFraction = hit.Fraction;
                        hitPos = hit.Position;
                        normal = hit.SurfaceNormal;
                        hasValidSurface = true;
                    }
                }
            }
            
            // Обязательно освобождаем память DOTS
            hits.Dispose();

            if (!hasValidSurface) return;

            // 2. РАЗМЕЩЕНИЕ И ПРОВЕРКА НАЛОЖЕНИЙ
            foreach (var (transform, snapSettings, ghostTag, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<GridSnapSettings>, RefRW<GhostTag>>().WithEntityAccess().WithAll<GhostTag>())
            {
                float3 snapPos = hitPos;

                if (snapSettings.ValueRO.IsEnabled && snapSettings.ValueRO.Step > 0)
                {
                    snapPos = math.round(snapPos / snapSettings.ValueRO.Step) * snapSettings.ValueRO.Step;
                }

                quaternion targetRotation = quaternion.LookRotationSafe(math.forward(), normal);
                float3 finalPos = snapPos;

                if (SystemAPI.HasComponent<PlacementPivot>(entity))
                {
                    var pivot = SystemAPI.GetComponent<PlacementPivot>(entity);
                    finalPos += math.rotate(targetRotation, pivot.Value);
                }

                transform.ValueRW.Position = finalPos;
                transform.ValueRW.Rotation = targetRotation;

                // --- ЛОГИКА ПРОВЕРКИ НАЛОЖЕНИЙ ---
                bool isValid = true;
                if (SystemAPI.HasComponent<PlacementBounds>(entity))
                {
                    var bounds = SystemAPI.GetComponent<PlacementBounds>(entity);
                    
                    foreach (var (otherTrans, otherBounds) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlacementBounds>>()
                        .WithAll<PlaceableTag>().WithNone<GhostTag>())
                    {
                        float3 diff = math.abs(finalPos - otherTrans.ValueRO.Position);
                        float3 minAllowedDistance = (bounds.Size + otherBounds.ValueRO.Size) * 0.5f;

                        if (diff.x < minAllowedDistance.x && diff.y < minAllowedDistance.y && diff.z < minAllowedDistance.z)
                        {
                            isValid = false;
                            break;
                        }
                    }
                }

                ghostTag.ValueRW.IsValid = isValid;
            }
        }
    }
}