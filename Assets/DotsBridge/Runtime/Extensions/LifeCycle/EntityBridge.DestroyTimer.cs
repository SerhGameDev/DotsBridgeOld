using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Добавляет компонент таймера смерти ко всем сущностям в батче.
        /// </summary>
        public static EntityBatch SetDestroyTimer(this EntityBatch batch, float lifetime)
        {
            // Пакетное добавление компонента (максимально быстро)
            batch.Manager.AddComponent<DestroyTimer>(batch.Entities.AsArray());

            var timerData = new DestroyTimer { Value = lifetime };

            // Устанавливаем значение
            foreach (var entity in batch.Entities)
            {
                batch.Manager.SetComponentData(entity, timerData);
            }
            return batch;
        }

        /// <summary>
        /// Добавляет установку таймера смерти в цепочку DotsCommand.
        /// </summary>
        public static DotsCommand SetDestroyTimer(this DotsCommand cmd, float lifetime)
        {
            return cmd.Do(batch => batch.SetDestroyTimer(lifetime));
        }
    }
}