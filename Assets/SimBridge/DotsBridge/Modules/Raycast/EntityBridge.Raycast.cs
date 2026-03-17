using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Пускает луч и возвращает ListEntity с одной сущностью, если попали в объект с компонентом T.
        /// </summary>
        public static ListEntity Raycast<T>(this BridgeWorld bridge, float3 start, float3 end)
            where T : unmanaged, IComponentData
        {
            var hit = PerformRaycast<T>(bridge, start, end);
            var batch = new ListEntity(bridge, Allocator.Temp);

            if (hit.Entity != Entity.Null)
            {
                batch.Entities.Add(hit.Entity);
            }

            return batch;
        }

        /// <summary>
        /// Возвращает точку соприкосновения луча с объектом, имеющим тег T.
        /// </summary>
        public static float3 RaycastGetPoint<T>(this BridgeWorld bridge, float3 start, float3 end)
            where T : unmanaged, IComponentData
        {
            var hit = PerformRaycast<T>(bridge, start, end);
            // Если попали — возвращаем позицию, если нет — нулевой вектор
            return hit.Entity != Entity.Null ? hit.Position : float3.zero;
        }

        // =========================================================
        // ВНУТРЕННИЙ ФИЗИЧЕСКИЙ ДВИЖОК
        // =========================================================

        private static RaycastHit PerformRaycast<T>(BridgeWorld bridge, float3 start, float3 end)
            where T : unmanaged, IComponentData
        {
            var physicsQuery = bridge.Manager.CreateEntityQuery(typeof(PhysicsWorldSingleton));

            if (physicsQuery.IsEmptyIgnoreFilter)
            {
                UnityEngine.Debug.LogWarning("[DotsBridge] PhysicsWorldSingleton не найден! Убедитесь, что физика инициализирована.");
                return default;
            }

            // 2. Достаем синглтон физики через запрос
            var physicsWorld = physicsQuery.GetSingleton<PhysicsWorldSingleton>();

            var input = new RaycastInput
            {
                Start = start,
                End = end,
                Filter = CollisionFilter.Default
            };

            // 3. Пускаем луч
            if (physicsWorld.CastRay(input, out var hit))
            {
                // Проверяем наш "специальный тег"
                if (bridge.Manager.HasComponent<T>(hit.Entity))
                {
                    return hit;
                }
            }

            return default;
        }/// <summary>
         /// Пускает луч из позиции мыши и возвращает батч с сущностью (если попали).
         /// </summary>
        public static ListEntity RaycastMouse<T>(this BridgeWorld bridge, float distance = 100f)
            where T : unmanaged, IComponentData
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return bridge.Raycast<T>(ray.origin, (float3)ray.origin + (float3)ray.direction * distance);
        }

        /// <summary>
        /// Возвращает точку попадания луча из мыши в объект с тегом T.
        /// </summary>
        public static float3 GetMousePoint<T>(this BridgeWorld bridge, float distance = 100f)
            where T : unmanaged, IComponentData
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return bridge.RaycastGetPoint<T>(ray.origin, (float3)ray.origin + (float3)ray.direction * distance);
        }

        /// <summary>
        /// Возвращает SingleEntity, на которую сейчас наведен курсор. 
        /// Если наведения нет, вернет "пустую" SingleEntity (Entity.Null).
        /// </summary>
        public static SingleEntity GetEntityUnderMouse<T>(this BridgeWorld bridge, float distance = 100f)
            where T : unmanaged, IComponentData
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit = PerformRaycast<T>(bridge, ray.origin, (float3)ray.origin + (float3)ray.direction * distance);
            return new SingleEntity(hit.Entity, bridge);
        }/// <summary>
         /// Возвращает true, если курсор мыши наведен на объект с компонентом T.
         /// </summary>
        public static bool IsMouseOver<T>(this BridgeWorld bridge, float distance = 100f)
            where T : unmanaged, IComponentData
        {
            // Мы просто переиспользуем логику луча, проверяя результат на Entity.Null
            var ray = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            var hit = PerformRaycast<T>(bridge, ray.origin, (Unity.Mathematics.float3)ray.origin + (Unity.Mathematics.float3)ray.direction * distance);

            return hit.Entity != Unity.Entities.Entity.Null;
        }
    }
}