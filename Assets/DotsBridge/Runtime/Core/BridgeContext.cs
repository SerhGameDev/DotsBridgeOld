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
            int hash = EntityBridge.GetHash(name);

            // Если префабы еще не кэшированы для этого мира, ищем их
            if (!State.IsPrefabBufferCached)
            {
                // Ищем сущность с буфером префабов (ее создает PrefabContainerAuthoring)
                var query = State.Manager.CreateEntityQuery(typeof(PrefabRegistryElement));

                if (!query.IsEmptyIgnoreFilter)
                {
                    // Читаем буфер
                    var buffer = query.GetSingletonBuffer<PrefabRegistryElement>(true);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        State.Prefabs[buffer[i].NameHash] = buffer[i].PrefabEntity;
                    }
                    State.IsPrefabBufferCached = true;
                }
                else
                {
                    UnityEngine.Debug.LogError($"[DotsBridge] В мире {State.World.Name} не найден контейнер префабов! Убедитесь, что объект с PrefabContainerAuthoring лежит в SubScene.");
                    return Entity.Null;
                }
            }

            // Выдаем префаб из кэша
            if (State.Prefabs.TryGetValue(hash, out Entity prefab))
            {
                return prefab;
            }

            UnityEngine.Debug.LogError($"[DotsBridge] Префаб '{name}' не найден в реестре мира {State.World.Name}!");
            return Entity.Null;
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
        public readonly Dictionary<int, Entity> Prefabs = new Dictionary<int, Entity>();
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
            Prefabs.Clear(); // Очищаем префабы
        }
    }
}