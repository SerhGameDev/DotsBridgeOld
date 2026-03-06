using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    public class BridgeState : IDisposable
    {
        public readonly World World;
        public readonly EntityManager Manager;

        public readonly Dictionary<int, EntityContainer> Containers = new Dictionary<int, EntityContainer>();
        public readonly Dictionary<string, ComponentType> TagRegistry = new Dictionary<string, ComponentType>();

        // --- НОВЫЕ ПОЛЯ ДЛЯ ПРЕФАБОВ ---
        public readonly Dictionary<int, Entity> Prefabs = new Dictionary<int, Entity>();
        public bool IsPrefabBufferCached = false;

        public BridgeState(World world)
        {
            World = world;
            Manager = world.EntityManager;
        }

        public void Dispose()
        {
            foreach (var container in Containers.Values)
                container.Dispose();

            Containers.Clear();
            TagRegistry.Clear();
            Prefabs.Clear(); // Очищаем префабы
        }
        internal void AddEntityToContainer(int hash, Entity entity)
        {
            if (!Containers.TryGetValue(hash, out var container))
            {
                container = new EntityContainer(hash, this);
                Containers.Add(hash, container);
            }
            container.Entities.Add(entity);
        }

        internal void RemoveEntityFromContainer(int hash, Entity entity)
        {
            if (Containers.TryGetValue(hash, out var container))
            {
                int index = container.Entities.IndexOf(entity);
                if (index != -1)
                {
                    container.Entities.RemoveAtSwapBack(index);
                }
            }
        }
    }
}