using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace DotsBridge
{
    /// <summary>
    /// Контейнер для хранения списка сущностей, привязанных к определенному ID.
    /// ВНИМАНИЕ: Требует вызова Dispose() при удалении системы или выходе из игры!
    /// Скорость: Высокая.
    /// Лимит: Хранит долгоживущие NativeList (Allocator.Persistent).
    /// </summary>
    public class EntityGroup : IDisposable
    {
        public readonly int ID;
        public NativeList<Entity> Entities;
        public readonly EntityManager Manager;
        public JobHandle CleanupHandle;

        public EntityGroup(int id, EntityManager manager)
        {
            ID = id;
            Manager = manager;
            Entities = new NativeList<Entity>(Allocator.Persistent);
        }

        public EntityBatch GetBatch()
        {
            CleanupHandle.Complete();
            return new EntityBatch(Entities, Manager);
        }

        public void Dispose()
        {
            CleanupHandle.Complete();

            if (Entities.IsCreated)
                Entities.Dispose();
        }
    }
}