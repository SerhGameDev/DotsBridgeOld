using System.Collections.Generic;
using Unity.Entities;
using System;

namespace DotsBridge
{
    public static partial class Dots
    {
        public static Dictionary<int, Action<Entity, Entity>> CollisionEvents = new Dictionary<int, Action<Entity, Entity>>();

        /// <summary>
        /// Подписаться на столкновение для определенной группы.
        /// </summary>
        public static void SubscribeToCollision(string groupID, Action<Entity, Entity> callback)
        {
            int hash = GetHash(groupID);
            if (!CollisionEvents.ContainsKey(hash))
                CollisionEvents[hash] = callback;
            else
                CollisionEvents[hash] += callback;
        }

        /// <summary>
        /// Отписаться от событий столкновения.
        /// </summary>
        public static void UnsubscribeFromCollision(string groupID, Action<Entity, Entity> callback)
        {
            int hash = GetHash(groupID);
            if (CollisionEvents.ContainsKey(hash))
                CollisionEvents[hash] -= callback;
        }

    }
}
