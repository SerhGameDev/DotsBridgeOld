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