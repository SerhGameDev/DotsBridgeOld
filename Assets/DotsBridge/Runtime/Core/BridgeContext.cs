using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    /// <summary>
    /// Структура-помощник, которая знает, в каком мире мы сейчас работаем.
    /// </summary>
    public readonly struct BridgeContext
    {
        public readonly BridgeState State;

        public BridgeContext(BridgeState state)
        {
            State = state;
        }

        // --- СПАВН ---

        public SpawnerBuilder BeginSpawn(Entity prefab)
        {
            return new SpawnerBuilder(State, prefab);
        }

        public SpawnerBuilder BeginSpawn(string prefabName)
        {
            return new SpawnerBuilder(State, GetPrefab(prefabName));
        }

        // --- ИДЕНТИФИКАЦИЯ И ПРЕФАБЫ ---

        public Entity GetPrefab(string name)
        {
            // Берем буфер префабов из State текущего мира (логика из твоего старого EntityBridge.Prefabs.cs)
            // ... 
            return Entity.Null; // Замени на реальную логику поиска по _prefabBuffer внутри State
        }

        public EntityBatch GetById(string id)
        {
            int hash = EntityBridge.GetHash(id);

            // Ищем контейнер только в словаре ЭТОГО мира
            if (!State.Containers.TryGetValue(hash, out var container))
            {
                container = new EntityContainer(hash, State.Manager);

                // Здесь логика первичного заполнения через FindInternal...
                // ...

                State.Containers.Add(hash, container);
            }
            return container.GetBatch();
        }

        public EntityBatch GetByTags(params string[] tags)
        {
            // Аналогично, читаем TagRegistry из State ЭТОГО мира
            // ...
            return new EntityBatch(default, State.Manager); // Заглушка
        }

        // --- ПОИСК (QUERIES) ---

        public EntityBatch Get<T1>() where T1 : struct, IComponentData
        {
            var query = State.Manager.CreateEntityQuery(ComponentType.ReadOnly<T1>());
            var entityArray = query.ToEntityArray(Allocator.Temp);

            var entityList = new NativeList<Entity>(entityArray.Length, Allocator.Persistent);
            entityList.AddRange(entityArray);

            entityArray.Dispose();
            query.Dispose();

            // Обрати внимание: мы передаем State.Manager, а не глобальный Manager
            return new EntityBatch(entityList, State.Manager);
        }

        public EntityBatch Get<T1, T2>()
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
        {
            var query = State.Manager.CreateEntityQuery(ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>());
            // ... аналогичная логика создания батча ...
            return new EntityBatch(default, State.Manager); // Заглушка
        }
    }
    /// <summary>
     /// Изолированное состояние моста для конкретного мира (Server, Client и т.д.)
     /// </summary>
    public class BridgeState : IDisposable
    {
        public readonly World World;
        public readonly EntityManager Manager;

        // Эти словари больше не static! Они живут внутри конкретного мира.
        public readonly Dictionary<int, EntityContainer> Containers = new Dictionary<int, EntityContainer>();
        public readonly Dictionary<string, ComponentType> TagRegistry = new Dictionary<string, ComponentType>();

        // Буфер префабов тоже переезжает сюда
        public bool IsPrefabBufferCached = false;

        public BridgeState(World world)
        {
            World = world;
            Manager = world.EntityManager;
        }

        public void Dispose()
        {
            foreach (var container in Containers.Values)
            {
                container.Dispose();
            }
            Containers.Clear();
            TagRegistry.Clear();
        }
    }
}