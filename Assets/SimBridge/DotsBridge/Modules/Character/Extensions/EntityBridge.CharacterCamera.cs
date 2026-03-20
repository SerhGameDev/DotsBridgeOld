using UnityEngine;
using Unity.Entities;
using DotsBridge.Character;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Создает новую уникальную камеру для персонажа и привязывает её к нему.
        /// Отключает предыдущую Camera.main, если она есть.
        /// </summary>
        public static SingleEntity AttachFirstPersonCamera(this SingleEntity entity)
        {
            if (entity.Entity == Entity.Null) return entity;

            // 1. Отключаем текущую главную камеру на сцене (чтобы не было конфликтов рендера)
            if (Camera.main != null)
            {
                Camera.main.enabled = false;
            }

            // 2. Создаем новую камеру
            var cameraObject = new GameObject($"FP_Camera_{entity.Entity.Index}");
            var newCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera"; // Делаем её новой главной

            // Настройки камеры по умолчанию
            newCamera.nearClipPlane = 0.05f;

            // 3. Добавляем ссылку в сущность
            entity.Manager.AddComponentData(entity.Entity, new CharacterCameraLink { Camera = newCamera });

            Debug.Log($"[DotsBridge] Создана и привязана новая камера для сущности {entity.Entity.Index}");
            return entity;
        }

        /// <summary>
        /// Уничтожает привязанную камеру (например, при отмене контроля или смерти).
        /// </summary>
        public static SingleEntity DetachCamera(this SingleEntity entity)
        {
            if (entity.Entity == Entity.Null) return entity;

            if (entity.Manager.HasComponent<CharacterCameraLink>(entity.Entity))
            {
                var link = entity.Manager.GetComponentData<CharacterCameraLink>(entity.Entity);
                if (link.Camera != null)
                {
                    Object.Destroy(link.Camera.gameObject);
                }
                entity.Manager.RemoveComponent<CharacterCameraLink>(entity.Entity);
            }
            return entity;
        }
    }
}