using System;
using Unity.Entities;
using Unity.Collections;

namespace DotsBridge
{
    public static partial class EntityBridge
    {

        /// <summary>
        /// Подготавливает сущности к смерти: добавляет DeathEvent и выключает его.
        /// </summary>
        public static ListEntity AddDeathEvent(this ListEntity batch)
        {
            if (batch.Count == 0) return batch;

            // Массовое добавление компонента
            batch.Manager.AddComponent<DeathEvent>(batch.Entities.AsArray());

            // Выключаем компоненты (используем встроенный метод ListEntity для чистоты)
            return batch.SetEnabled<DeathEvent>(false);
        }

        /// <summary>
        /// Активирует DeathEvent. Сущность "умрет" для систем в конце кадра.
        /// </summary>
        public static ListEntity TriggerDeath(this ListEntity batch)
        {
            return batch.SetEnabled<DeathEvent>(true);
        }

        /// <summary>
        /// Назначает префаб, который заспавнится при смерти (взрыв, обломки и т.д.).
        /// </summary>
        public static ListEntity SetSpawnOnDeath(this ListEntity batch, Entity prefabToSpawn)
        {
            if (batch.Count == 0 || prefabToSpawn == Entity.Null) return batch;

            batch.Manager.AddComponent<SpawnOnDeath>(batch.Entities.AsArray());

            // Записываем данные (используем наш оптимизированный SetComponent из ListEntity)
            return batch.AddComponent(new SpawnOnDeath { Prefab = prefabToSpawn });
        }

        // =========================================================
        // OOP CALLBACKS (BRIDGE)
        // =========================================================

        /// <summary>
        /// Подписывает C# метод на уничтожение сущностей. 
        /// Реестр выбирается автоматически на основе мира, в котором живут сущности.
        /// </summary>
        public static ListEntity SubscribeOnDeath(this ListEntity batch, Action<Entity> onDeathAction)
        {
            if (batch.Count == 0 || onDeathAction == null || batch.Word == null) return batch;

            var events = batch.Word.OnDestroyEvents;

            for (int i = 0; i < batch.Entities.Length; i++)
            {
                var entity = batch.Entities[i];
                if (events.ContainsKey(entity))
                {
                    events[entity] += onDeathAction;
                }
                else
                {
                    events[entity] = onDeathAction;
                }
            }
            return batch;
        }
    }
}