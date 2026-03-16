using Unity.Entities;
using UnityEngine;


namespace DotsBridge
{
    public static partial class EntityBridge
    { 
        private static Entity GetPrefab(this BridgeWorld world, string name)
        {
            if (world == null)
            {
                Debug.LogError($"[DotsBridge] Не удалось найти активный Registry для поиска префаба '{name}'");
                return Entity.Null;
            }

            if (!world.IsPrefabBufferCached)
            {
                var query = world.Manager.CreateEntityQuery(typeof(PrefabRegistryElement));

                if (query.IsEmpty)
                {
                    Debug.LogError($"[DotsBridge] PrefabContainer не найден в мире {world.World.Name}!");
                    return Entity.Null;
                }

                var buffer = query.GetSingletonBuffer<PrefabRegistryElement>(true);
                foreach (var element in buffer)
                {
                    world.Prefabs[element.NameHash] = element.PrefabEntity;
                }
                world.IsPrefabBufferCached = true;
            }

            int hash = GetHash(name);
            if (world.Prefabs.TryGetValue(hash, out var prefab))
            {
                return prefab;
            }

            Debug.LogError($"[DotsBridge] Префаб '{name}' не найден в реестре {world.World.Name}!");
            return Entity.Null;
        }
    }
}