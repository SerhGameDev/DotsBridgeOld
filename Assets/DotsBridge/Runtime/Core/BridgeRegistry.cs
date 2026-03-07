using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    /// <summary>
    /// Изолированное состояние моста для конкретного мира (Server, Client и т.д.)
    /// </summary>
    public class BridgeRegistry : IDisposable
    {
        public readonly World World;
        public readonly EntityManager Manager;

        // Эти словари больше не static! Они живут внутри конкретного мира.
        public readonly Dictionary<int, EntityGroup> Groups = new Dictionary<int, EntityGroup>();
        public readonly Dictionary<string, ComponentType> TagRegistry = new Dictionary<string, ComponentType>();
        public readonly Dictionary<int, Entity> Prefabs = new Dictionary<int, Entity>();
        // Буфер префабов тоже переезжает сюда
        public bool IsPrefabBufferCached = false;
        public readonly Dictionary<Entity, Action<Entity>> OnDestroyEvents = new Dictionary<Entity, Action<Entity>>();
        public BridgeRegistry(World world)
        {
            World = world;
            Manager = world.EntityManager;
        }

        public void Dispose()
        {
            foreach (var container in Groups.Values)
            {
                container.Dispose();
            }
            Groups.Clear();
            TagRegistry.Clear(); 
            Prefabs.Clear(); // Очищаем префабы
        }// --- ВНУТРЕННИЕ МЕТОДЫ ДЛЯ РЕАКТИВНОЙ СИСТЕМЫ ---

        internal void AddEntityToGroup(int hash, Entity entity)
        {
            if (!Groups.TryGetValue(hash, out var group))
            {
                group = new EntityGroup(hash, Manager);
                Groups.Add(hash, group);
            }
            group.Entities.Add(entity);
        }

        internal void RemoveEntityFromGroup(int hash, Entity entity)
        {
            if (Groups.TryGetValue(hash, out var group))
            {
                int index = group.Entities.IndexOf(entity);
                if (index != -1)
                {
                    group.Entities.RemoveAtSwapBack(index);
                }
            }
        }
    }

}