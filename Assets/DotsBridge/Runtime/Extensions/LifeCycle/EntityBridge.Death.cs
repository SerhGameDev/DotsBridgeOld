using System;
using Unity.Entities;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

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
        /// Безопасно для мультиплеера: подписка сохраняется в реестре конкретного мира.
        /// </summary>
        public static EntityBatch SubscribeOnDeathForServer(this EntityBatch batch, Action<Entity> onDeathAction)
        {
            return SubscribeOnDeath(batch, onDeathAction, ServerRegistry);
        }
        /// <summary>
        /// [OOP Bridge] Подписывает классический C# метод на событие смерти этих сущностей.
        /// Безопасно для мультиплеера: подписка сохраняется в реестре конкретного мира.
        /// </summary>
        public static EntityBatch SubscribeOnDeathForClient(this EntityBatch batch, Action<Entity> onDeathAction)
        {
            return SubscribeOnDeath(batch, onDeathAction, ClientRegistry);
        }

        /// <summary>
        /// [OOP Bridge] Подписывает классический C# метод на событие смерти этих сущностей.
        /// Безопасно для мультиплеера: подписка сохраняется в реестре конкретного мира.
        /// </summary>
        public static EntityBatch SubscribeOnDeath(this EntityBatch batch, Action<Entity> onDeathAction, BridgeRegistry registry )
        {
            if (registry == null) return batch;

            foreach (var entity in batch.Entities)
            {
                if (registry.OnDestroyEvents.ContainsKey(entity))
                {
                    registry.OnDestroyEvents[entity] += onDeathAction;
                }
                else
                {
                    registry.OnDestroyEvents[entity] = onDeathAction;
                }
            }
            return batch;
        }

        public static DotsCommand SubscribeOnDeathForClient(this DotsCommand cmd, Action<Entity> onDeathAction) => cmd.Do(b => b.SubscribeOnDeath(onDeathAction, ClientRegistry));

        public static DotsCommand SubscribeOnDeathForServer(this DotsCommand cmd, Action<Entity> onDeathAction) => cmd.Do(b => b.SubscribeOnDeath(onDeathAction, ServerRegistry));

    }
}