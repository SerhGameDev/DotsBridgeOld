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
            
            // НОВОЕ: Запоминаем конкретную поверхность, в которую попал луч
            Entity hitEntity = Entity.Null; 
int requiredLayerHash = 0;
            bool requiresLayer = false;

            foreach (var layer in SystemAPI.Query<RefRO<PlacementLayer>>().WithAll<GhostTag>())
            {
                requiresLayer = true;
                requiredLayerHash = layer.ValueRO.Hash;
                break; // Фантом у нас один, берем первый попавшийся
            }

            var input = new RaycastInput
            {
                Start = ray.origin,
                End = (float3)ray.origin + (float3)ray.direction * 100f,
                Filter = CollisionFilter.Default
            };

            var hits = new NativeList<RaycastHit>(Allocator.Temp);

            if (physicsWorld.CollisionWorld.CastRay(input, ref hits))
            {
                float closestFraction = float.MaxValue;

                for (int i = 0; i < hits.Length; i++)
                {
                    var hit = hits[i];

                    if (SystemAPI.HasComponent<SurfaceTag>(hit.Entity) && hit.Fraction < closestFraction)
                    {
                        // --- НОВОЕ: Проверка совпадения слоев ---
                        bool layerMatch = true;
                        if (requiresLayer)
                        {
                            if (SystemAPI.HasComponent<PlacementLayer>(hit.Entity))
                            {
                                layerMatch = SystemAPI.GetComponent<PlacementLayer>(hit.Entity).Hash == requiredLayerHash;
                            }
                            else
                            {
                                // Если фантому нужен слой, а у поверхности его нет — отклоняем
                                layerMatch = false; 
                            }
                        }

                        if (layerMatch)
                        {
                            closestFraction = hit.Fraction;
                            hitPos = hit.Position;
                            normal = hit.SurfaceNormal;
                            hitEntity = hit.Entity;
                            hasValidSurface = true;
                        }
                    }
                }
            }
            hits.Dispose();

            if (!hasValidSurface) return;

            // 2. РАЗМЕЩЕНИЕ
            foreach (var (transform, snapSettings, ghostTag, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<GridSnapSettings>, RefRW<GhostTag>>().WithEntityAccess().WithAll<GhostTag>())
            {
                float3 finalPos = hitPos;
                quaternion targetRotation;

                // --- ЛОГИКА ОГРАНИЧЕНИЯ ПО ЛИНИИ (DIN-рейка) ---
                if (SystemAPI.HasComponent<RailSurface>(hitEntity))
                {
                    var rail = SystemAPI.GetComponent<RailSurface>(hitEntity);
                    var railMatrix = SystemAPI.GetComponent<LocalToWorld>(hitEntity).Value;
                    
                    float4x4 worldToLocal = math.inverse(railMatrix);
                    float3 localHit = math.transform(worldToLocal, hitPos);
                    
                    // СНАППИНГ: Делаем округление в ЛОКАЛЬНЫХ координатах рейки
                    if (snapSettings.ValueRO.IsEnabled && snapSettings.ValueRO.Step > 0)
                    {
                        localHit = math.round(localHit / snapSettings.ValueRO.Step) * snapSettings.ValueRO.Step;
                    }

                    // Прибиваем к центру
                    if (rail.AllowedAxis == RailAxis.X) { localHit.y = 0; localHit.z = 0; }
                    else if (rail.AllowedAxis == RailAxis.Y) { localHit.x = 0; localHit.z = 0; }
                    else if (rail.AllowedAxis == RailAxis.Z) { localHit.x = 0; localHit.y = 0; }
                    
                    // Возвращаем в мировые
                    finalPos = math.transform(railMatrix, localHit);
                    normal = math.normalize(railMatrix.c1.xyz);
                    targetRotation = quaternion.LookRotationSafe(math.forward(), normal);
                }
                // --- ЛОГИКА ОБЫЧНОЙ ПОВЕРХНОСТИ ---
                else
                {
                    // СНАППИНГ: Делаем округление в МИРОВЫХ координатах
                    if (snapSettings.ValueRO.IsEnabled && snapSettings.ValueRO.Step > 0)
                    {
                        finalPos = math.round(finalPos / snapSettings.ValueRO.Step) * snapSettings.ValueRO.Step;
                    }
                    targetRotation = quaternion.LookRotationSafe(math.forward(), normal);
                }

                // Применяем Pivot Offset
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