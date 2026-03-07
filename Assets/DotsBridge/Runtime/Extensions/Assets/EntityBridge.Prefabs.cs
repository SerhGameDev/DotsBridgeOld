using Unity.Entities;
using UnityEngine;


namespace DotsBridge
{
    public static partial class EntityBridge
    {        /// Если registry не указан, пытается найти в активном мире.
        /// </summary>

        public static Entity GetPrefabServer(string name) => GetPrefab(name, ServerRegistry);
        public static Entity GetPrefabClient(string name) => GetPrefab(name, ClientRegistry);

        private static Entity GetPrefab(string name, BridgeRegistry registry)
        {
            // Если реестр не передан, берем текущий активный (например, серверный по умолчанию)
            var targetRegistry = registry ?? GetActiveRegistry();

            if (targetRegistry == null)
            {
                Debug.LogError($"[DotsBridge] Не удалось найти активный Registry для поиска префаба '{name}'");
                return Entity.Null;
            }

            // 1. Если префабы для ЭТОГО мира еще не в кэше — заполняем
            if (!targetRegistry.IsPrefabBufferCached)
            {
                var query = targetRegistry.Manager.CreateEntityQuery(typeof(PrefabRegistryElement));

                if (query.IsEmpty)
                {
                    Debug.LogError($"[DotsBridge] PrefabContainer не найден в мире {targetRegistry.World.Name}!");
                    return Entity.Null;
                }

                var buffer = query.GetSingletonBuffer<PrefabRegistryElement>(true);
                foreach (var element in buffer)
                {
                    targetRegistry.Prefabs[element.NameHash] = element.PrefabEntity;
                }
                targetRegistry.IsPrefabBufferCached = true;
            }

            // 2. Ищем в кэше конкретного реестра
            int hash = GetHash(name);
            if (targetRegistry.Prefabs.TryGetValue(hash, out var prefab))
            {
                return prefab;
            }

            Debug.LogError($"[DotsBridge] Префаб '{name}' не найден в реестре {targetRegistry.World.Name}!");
            return Entity.Null;
        }
    }
}