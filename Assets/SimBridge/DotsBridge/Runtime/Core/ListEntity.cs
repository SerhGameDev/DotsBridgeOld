using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DotsBridge
{
    public interface IEntityFilter
    {
        bool Execute(SingleEntity entity);
    }
    public readonly partial struct ListEntity : IDisposable
    {
        public readonly NativeList<Entity> Entities;
        public readonly BridgeWorld Word;
        public EntityManager Manager => Word.Manager;
        private readonly bool _isOwner;
        public int Count => Entities.IsCreated ? Entities.Length : 0;

        public ListEntity(BridgeWorld bridge, Allocator allocator = Allocator.Temp)
        {
            Entities = new NativeList<Entity>(allocator);
            Word = bridge;
            _isOwner = true;
        }

        internal ListEntity(BridgeWorld bridge, NativeList<Entity> existingList, bool isOwner = false)
        {
            Entities = existingList;
            Word = bridge;
            _isOwner = isOwner;
        }

        /// <summary>
        /// Универсальный фильтр. Оставляет в батче только те сущности, 
        /// которые удовлетворяют переданному условию.
        /// </summary>
        public ListEntity Filter(Func<SingleEntity, bool> condition)
        {
            if (Manager == default || !Entities.IsCreated || condition == null)
                return this;

            // Идем с конца, так как удаляем элементы "на месте"
            for (int i = Entities.Length - 1; i >= 0; i--)
            {
                var entity = Entities[i];

                if (!condition(new SingleEntity(entity, Word)))
                {
                    Entities.RemoveAtSwapBack(i);
                }
            }

            return this;
        }
        /// <summary>
        /// Фильтрует батч, оставляя только те сущности, где данные компонента T 
        /// соответствуют заданному условию.
        /// </summary>
        /// <example> batch.FilterByValue<Health>(h => h.Amount > 0); </example>
        public ListEntity FilterByValue<T>(Func<T, bool> predicate) where T : unmanaged, IComponentData
        {
            if (!Entities.IsCreated || Entities.Length == 0) return this;

            for (int i = Entities.Length - 1; i >= 0; i--)
            {
                var entity = Entities[i];
                if (Manager.HasComponent<T>(entity))
                {
                    var componentData = Manager.GetComponentData<T>(entity);
                    if (!predicate(componentData))
                    {
                        Entities.RemoveAtSwapBack(i);
                    }
                }
                else
                {
                    // Если компонента вообще нет — удаляем из батча
                    Entities.RemoveAtSwapBack(i);
                }
            }
            return this;
        }

        public ListEntity Filter<T>(T filter) where T : struct, IEntityFilter
        {
            if (Manager == default || !Entities.IsCreated) return this;

            for (int i = Entities.Length - 1; i >= 0; i--)
            {
                var single = new SingleEntity(Entities[i], Word);

                if (!filter.Execute(single))
                {
                    Entities.RemoveAtSwapBack(i);
                }
            }

            return this;
        }

        /// <summary>
        /// Вариант 1: Создание сущностей на основе существующего префаба.
        /// Самый быстрый способ в ECS для создания множества копий.
        /// </summary>
        public ListEntity Instantiate(Entity prefab, int count = 1)
        {
            if (count <= 0 || prefab == Entity.Null) return this;

            // Используем временный массив для получения созданных сущностей
            using var spawned = new NativeArray<Entity>(count, Allocator.Temp);
            Manager.Instantiate(prefab, spawned);

            // Добавляем их в наш батч
            Entities.AddRange(spawned);
            return this;
        }  

        /// <summary>
        /// Вариант 2: Создание "пустышек" (пустых сущностей без префаба).
        /// Можно передать архетип, если нужно заранее задать набор компонентов.
        /// </summary>
        public ListEntity CreateEmpty(int count = 1, EntityArchetype archetype = default)
        {
            if (count <= 0) return this;

            using var spawned = new NativeArray<Entity>(count, Allocator.Temp);
            Manager.CreateEntity(archetype, spawned);

            Entities.AddRange(spawned);
            return this;
        }

        /// <summary>
        /// Вариант 3: Создание "пустышек" с автоматической установкой позиции.
        /// Автоматически добавляет компонент LocalTransform, если его нет в архетипе.
        /// </summary>
        public ListEntity CreateEmpty(int count, float3 position, EntityArchetype archetype = default)
        {
            if (count <= 0) return this;

            using var spawned = new NativeArray<Entity>(count, Allocator.Temp);
            Manager.CreateEntity(archetype, spawned);

            Manager.AddComponent<LocalTransform>(spawned);

            var transform = LocalTransform.FromPosition(position);
            for (int i = 0; i < spawned.Length; i++)
            {
                Manager.SetComponentData(spawned[i], transform);
            }

            Entities.AddRange(spawned);
            return this;
        }
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Entities.Length) return;

            var entity = Entities[index];
            DestroyEntityWithBridgeCleanup(entity);

            Entities.RemoveAtSwapBack(index);
        }

        public bool Remove(Entity entity)
        {
            int index = Entities.AsArray().IndexOf(entity);
            if (index == -1) return false;

            RemoveAt(index);
            return true;
        }

        public void ClearAndDestroy()
        {
            if (Entities.Length == 0) return;


            for (int i = 0; i < Entities.Length; i++)
            {
                ProcessBridgeCleanup(Entities[i]);
            }

            Manager.DestroyEntity(Entities.AsArray());
            Entities.Clear();
        }


        /// <summary>
        /// Вызывает события OnDestroy и очищает сущность в рамках ECS.
        /// </summary>
        private void DestroyEntityWithBridgeCleanup(Entity entity)
        {
            if (!Manager.Exists(entity)) return;

            ProcessBridgeCleanup(entity);
            Manager.DestroyEntity(entity);
        }

        /// <summary>
        /// Выполняет логику BridgeWorld (вызов событий, удаление из групп), не уничтожая саму сущность в ECS.
        /// </summary>
        private void ProcessBridgeCleanup(Entity entity)
        {
            if (Word.OnDestroyEvents.TryGetValue(entity, out var onDestroyAction))
            {
                onDestroyAction?.Invoke(entity);
                Word.OnDestroyEvents.Remove(entity);
            }
        }

        public void Dispose()
        {
            if (_isOwner && Entities.IsCreated)
            {
                Entities.Dispose();
            }
        }

        /// <summary>
        /// Добавляет компонент с данными всем сущностям в батче.
        /// Выполняется за 1 структурное изменение, что обеспечивает высокую производительность.
        /// </summary>
        public ListEntity AddComponent<T>(T data) where T : unmanaged, IComponentData
        {
            if (!Entities.IsCreated || Entities.Length == 0) return this;
            Manager.AddComponent<T>(Entities.AsArray());
            for (int i = 0; i < Entities.Length; i++)
            {
                Manager.SetComponentData(Entities[i], data);
            }
            return this;
        }

        /// <summary>
        /// Добавляет компонент-тег всем сущностям в батче.
        /// </summary>
        public ListEntity AddComponent<T>() where T : unmanaged, IComponentData
        {
            if (Entities.Length == 0) return this;
            Manager.AddComponent<T>(Entities.AsArray());
            return this;
        }
        /// <summary>
        /// Добавляет компонент-тег всем сущностям в батче.
        /// </summary>
        public ListEntity AddComponent<T>(bool IsEnebled) where T : unmanaged, IComponentData, IEnableableComponent
        {
            if (Entities.Length == 0) return this;
            Manager.AddComponent<T>(Entities.AsArray());
            for (int i = 0; i < Entities.Length; i++)
            {
                Manager.SetComponentEnabled<T>(Entities[i] ,IsEnebled);
            }
            return this;
        }

        /// <summary>
        /// Безопасный вариант: обновляет данные только у тех сущностей, у которых ЕСТЬ этот компонент.
        /// </summary>
        public ListEntity TrySetComponent<T>(T data) where T : unmanaged, IComponentData
        {
            if (!Entities.IsCreated || Entities.Length == 0) return this;

            for (int i = 0; i < Entities.Length; i++)
            {
                if (Manager.HasComponent<T>(Entities[i]))
                {
                    Manager.SetComponentData(Entities[i], data);
                }
            }

            return this;
        }   

        /// <summary>
        /// Удаляет компонент у всех сущностей в батче.
        /// </summary>
        public ListEntity RemoveComponent<T>() where T : unmanaged, IComponentData
        {
            if (Entities.Length == 0) return this;
            Manager.RemoveComponent<T>(Entities.AsArray());
            return this;
        }

        /// <summary>
        /// Включает или выключает компонент (поддерживает IEnableableComponent).
        /// </summary>
        public ListEntity SetEnabled<T>(bool enabled) where T : unmanaged, IEnableableComponent
        {
            if (Entities.Length == 0) return this;
            for (int i = 0; i < Entities.Length; i++)
            {
                Manager.SetComponentEnabled<T>(Entities[i], enabled);
            }
            return this;
        }

        /// <summary>
        /// Оставляет в списке только те сущности, у которых ЕСТЬ компонент T.
        /// </summary>
        public ListEntity With<T>() where T : unmanaged, IComponentData
        {
            if (Entities.Length == 0) return this;
            for (int i = Entities.Length - 1; i >= 0; i--)
            {
                if (!Manager.HasComponent<T>(Entities[i]))
                    Entities.RemoveAtSwapBack(i);
            }
            return this;
        }

        /// <summary>
        /// Оставляет в списке только те сущности, у которых НЕТ компонента T.
        /// </summary>
        public ListEntity Without<T>() where T : unmanaged, IComponentData
        {
            if (Entities.Length == 0) return this;
            for (int i = Entities.Length - 1; i >= 0; i--)
            {
                if (Manager.HasComponent<T>(Entities[i]))
                    Entities.RemoveAtSwapBack(i);
            }
            return this;
        }

        public ListEntity AsBatch(BridgeWorld bridge, NativeList<Entity> entities, bool isOwner = true)
        {
            return new ListEntity(bridge, entities, isOwner: false);
        }

        public ListEntity Clone(Allocator allocator = Allocator.Temp)
        {
            var newList = new NativeList<Entity>(Entities.Length, allocator);
            newList.AddRange(Entities.AsArray());

            // Используем внутренний конструктор
            return new ListEntity(Word, newList);
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
        public static List<T> CopyToManagedList<T>(this ListEntity batch) where T : unmanaged, IComponentData
        {
            var list = new List<T>(batch.Entities.Length);

            batch.ForEach<T>((entity, component) =>
            {
                list.Add(component);
            }); 
            return list;
        }

        public static SingleEntity First(this ListEntity entity)
        {
            return entity.Entities[0].ToSingleEntity(entity.Word);
        }
        public static DotsCommand Command(string id = "")
        {
            return new DotsCommand(id);
        }

    }
}