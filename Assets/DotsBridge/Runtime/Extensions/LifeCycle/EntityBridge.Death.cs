using System;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Добавляет компонент DeathEvent к сущностям и сразу его ВЫКЛЮЧАЕТ.
        /// Идеально вызывать сразу после Spawn().
        /// </summary>
        public static EntityBatch AddDeathEvent(this EntityBatch batch)
        {
            batch.Manager.AddComponent<DeathEvent>(batch.Entities.AsArray());

            // Сразу переводим в спящий режим
            foreach (var entity in batch.Entities)
            {
                batch.Manager.SetComponentEnabled<DeathEvent>(entity, false);
            }
            return batch;
        }

        public static DotsCommand AddDeathEvent(this DotsCommand cmd) => cmd.Do(b => b.AddDeathEvent());

        /// <summary>
        /// Триггер смерти: ВКЛЮЧАЕТ DeathEvent. 
        /// Сущность будет жить до конца текущего кадра, позволяя другим системам отреагировать.
        /// </summary>
        public static EntityBatch TriggerDeath(this EntityBatch batch)
        {
            foreach (var entity in batch.Entities)
            {
                batch.Manager.SetComponentEnabled<DeathEvent>(entity, true);
            }
            return batch;
        }

        public static DotsCommand TriggerDeath(this DotsCommand cmd) => cmd.Do(b => b.TriggerDeath());


        /// <summary>
        /// [DOTS] Указывает, какой Entity-префаб заспавнить на месте этой сущности при ее смерти (партиклы/взрыв).
        /// </summary>
        public static EntityBatch SetSpawnOnDeath(this EntityBatch batch, Entity prefabToSpawn)
        {
            batch.Manager.AddComponent<SpawnOnDeath>(batch.Entities.AsArray());
            foreach (var entity in batch.Entities)
            {
                batch.Manager.SetComponentData(entity, new SpawnOnDeath { Prefab = prefabToSpawn });
            }
            return batch;
        }

        public static DotsCommand SetSpawnOnDeath(this DotsCommand cmd, Entity prefab) => cmd.Do(b => b.SetSpawnOnDeath(prefab));

        /// <summary>
        /// [OOP Bridge] Подписывает классический C# метод на событие смерти этих сущностей.
        /// </summary>
        public static EntityBatch SubscribeOnDeath(this EntityBatch batch, Action<Entity> onDeathAction)
        {
            foreach (var entity in batch.Entities)
            {
                EntityBridge.RegisterDestroy(entity, onDeathAction);
            }
            return batch;
        }

        public static DotsCommand SubscribeOnDeath(this DotsCommand cmd, Action<Entity> action) => cmd.Do(b => b.SubscribeOnDeath(action));
    }
}