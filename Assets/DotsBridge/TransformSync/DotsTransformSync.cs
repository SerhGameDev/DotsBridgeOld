using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public static partial class Dots
    {
        /// <summary>
        /// Связывает Entity и GameObject.
        /// </summary>
        /// <param name="entity">Сущность в ECS</param>
        /// <param name="transform">Трансформ в мире Unity</param>
        /// <param name="mode">Кто кем управляет?</param>
        public static void Link(Entity entity, Transform transform, SyncSettings.Mode mode)
        {
            var manager = Manager;

            manager.AddComponentData(entity, new LinkedTransform { Transform = transform });

            manager.AddComponentData(entity, new SyncSettings
            {
                SyncMode = mode,
                SyncPosition = true,
                SyncRotation = true
            });
        }

        /// <summary>
        /// Разрывает связь.
        /// </summary>
        public static void Unlink(Entity entity)
        {
            var manager = Manager;
            if (manager.HasComponent<LinkedTransform>(entity))
                manager.RemoveComponent<LinkedTransform>(entity);

            if (manager.HasComponent<SyncSettings>(entity))
                manager.RemoveComponent<SyncSettings>(entity);
        }
    }
}
