using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    public readonly partial struct EntityBatch : IDisposable
    {
        public readonly NativeList<Entity> Entities;
        public readonly EntityManager Manager;

        public EntityBatch(NativeList<Entity> entities, EntityManager manager)
        {
            Entities = entities;
            Manager = manager;
        }

        // --- Методы добавления ---

        /// <summary>
        /// Создает новую сущность и добавляет её в батч.
        /// </summary>
        public Entity Create(EntityArchetype archetype = default)
        {
            var entity = Manager.CreateEntity(archetype);
            Entities.Add(entity);
            return entity;
        }

        /// <summary>
        /// Создает несколько сущностей на основе префаба и добавляет их в батч.
        /// </summary>
        public void Instantiate(Entity prefab, int count, Allocator allocator = Allocator.Temp)
        {
            using var newEntities = new NativeArray<Entity>(count, allocator);
            Manager.Instantiate(prefab, newEntities);
            Entities.AddRange(newEntities);
        }

        // --- Методы удаления ---

        /// <summary>
        /// Удаляет сущность из мира и из списка батча по индексу.
        /// Использует RemoveAtSwapBack для производительности.
        /// </summary>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Entities.Length) return;

            var entity = Entities[index];
            if (Manager.Exists(entity))
            {
                Manager.DestroyEntity(entity);
            }

            Entities.RemoveAtSwapBack(index);
        }

        /// <summary>
        /// Находит сущность в списке, уничтожает её в мире и удаляет из списка.
        /// </summary>
        public bool Remove(Entity entity)
        {
            int index = Entities.AsArray().IndexOf(entity);
            if (index == -1) return false;

            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Уничтожает все сущности батча в мире и очищает список.
        /// </summary>
        public void ClearAndDestroy()
        {
            if (Entities.Length == 0) return;

            // Эффективное массовое уничтожение через NativeArray
            Manager.DestroyEntity(Entities.AsArray());
            Entities.Clear();
        }

        public void Dispose()
        {
            if (Entities.IsCreated)
                Entities.Dispose();
        }

    }
    public static partial class EntityBridge
    {
        /// <summary>
        /// Экспортирует данные в стандартный C# List (например, для использования LINQ).
        /// ВНИМАНИЕ: Создает нагрузку на Garbage Collector!
        /// Лимит: Желательно не более 10 000 объектов во избежание фризов от сборщика мусора.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> CopyToManagedList<T>(this EntityBatch batch) where T : unmanaged, IComponentData
        {
            var list = new List<T>(batch.Entities.Length);

            batch.ForEach<T>((entity, component) =>
            {
                list.Add(component);
            });

            return list;
        }

        public static DotsCommand Command(string id)
        {
            return new DotsCommand(id);
        }

    }
}