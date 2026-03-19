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
            // Если в мире нет фантомов для размещения, система ничего не делает (оптимизация)
            if (SystemAPI.QueryBuilder().WithAll<GhostTag, LocalTransform>().Build().IsEmpty)
                return;

            // Получаем синглтон физического мира для рейкаста
            if (!SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorld))
                return;

            // Пускаем луч из камеры
            var camera = Camera.main;
            if (camera == null) return;

            var ray = camera.ScreenPointToRay(Input.mousePosition);
            var input = new RaycastInput
            {
                Start = ray.origin,
                End = ray.origin + ray.direction * 100f,
                Filter = CollisionFilter.Default
            };

            float3 targetPos = float3.zero;
            float3 normal = math.up();
            bool hasValidSurface = false;

            // Проверяем попадание
            if (physicsWorld.CastRay(input, out RaycastHit hit))
            {
                // Проверяем, есть ли у объекта, в который мы попали, тег SurfaceTag
                if (SystemAPI.HasComponent<SurfaceTag>(hit.Entity))
                {
                    targetPos = hit.Position;
                    normal = hit.SurfaceNormal;
                    hasValidSurface = true;
                }
            }

            if (!hasValidSurface) return;

            // Обновляем позиции всех фантомов (обычно он один, но foreach работает надежно)
            foreach (var (transform, snapSettings) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<GridSnapSettings>>().WithAll<GhostTag>())
            {
                float3 finalPos = targetPos;

                // Логика привязки к сетке (Snapping)
                if (snapSettings.ValueRO.IsEnabled && snapSettings.ValueRO.Step > 0)
                {
                    // Округляем позицию по шагу сетки
                    finalPos = math.round(finalPos / snapSettings.ValueRO.Step) * snapSettings.ValueRO.Step;
                }

                transform.ValueRW.Position = finalPos;

                // Ориентируем объект: вектор "вверх" объекта (Y) совпадает с нормалью поверхности
                // Вектор "вперед" (Z) пока оставляем по умолчанию, либо можно задать кастомный
                transform.ValueRW.Rotation = quaternion.LookRotationSafe(math.forward(), normal);
            }
        }
    }
}