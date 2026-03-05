using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Точка входа для спауна. Возвращает легковесный билдер.
        /// </summary>
        public static SpawnerBuilder Spawn(Entity prefab)
        {
            return new SpawnerBuilder(prefab);
        }
        public static SpawnerBuilder BeginSpawn(Entity prefab)
        {
            return new SpawnerBuilder(prefab);
        }
        public static SpawnerBuilder BeginSpawn(string namePrefab)
        {
            return new SpawnerBuilder(GetPrefab(namePrefab));
        }
    }
}
