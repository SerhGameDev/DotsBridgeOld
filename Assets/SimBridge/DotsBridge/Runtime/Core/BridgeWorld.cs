using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace DotsBridge
{
    public class BridgeWorld : IDisposable
    {
        public readonly World World;
        public readonly EntityManager Manager;

        public readonly Dictionary<int, NativeList<Entity>> Groups = new Dictionary<int, NativeList<Entity>>();
        public readonly Dictionary<string, ComponentType> TagRegistry = new Dictionary<string, ComponentType>();
        public readonly Dictionary<int, Entity> Prefabs = new Dictionary<int, Entity>();
        public readonly Dictionary<Entity, Action<Entity>> OnDestroyEvents = new Dictionary<Entity, Action<Entity>>();

        public bool IsPrefabBufferCached;
        public BridgeWorld(World world)
        {
            World = world;
            Manager = world.EntityManager;
        }

        public void Dispose()
        {
            foreach (var list in Groups.Values)
            {
                if (list.IsCreated) list.Dispose();
            }
            Groups.Clear();
            TagRegistry.Clear();
            Prefabs.Clear();
            OnDestroyEvents.Clear();
        }

        public void AddEntityToGroup(int hash, Entity entity)
        {
            if (!Groups.TryGetValue(hash, out var list))
            {
                list = new NativeList<Entity>(Allocator.Persistent);
                Groups.Add(hash, list);
            }
            list.Add(entity);
        }

        public void RemoveEntityFromGroup(int hash, Entity entity)
        {
            if (Groups.TryGetValue(hash, out var list))
            {
                int index = list.IndexOf(entity);
                if (index != -1)
                {
                    list.RemoveAtSwapBack(index);
                }
            }
        }

        /// <summary>
        /// Выдает безопасный фасад ListEntity для работы с группой
        /// </summary>
        public ListEntity GetGroupBatch(int hash)
        {
            if (Groups.TryGetValue(hash, out var list))
            {
                // Используем внутренний конструктор, который мы сделали ранее
                return new ListEntity(this, list);
            }
            // Возвращаем пустой батч, если группы нет
            return new ListEntity(this, Allocator.Temp);
        }
    }
}