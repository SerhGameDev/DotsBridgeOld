using Unity.Entities;
using DotsBridge.Character; 

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Берет одиночную сущность под контроль игрока (включает ActiveCharacterTag).
        /// </summary>
        public static SingleEntity Possess(this SingleEntity entity)
        {
            if (entity.Entity == Entity.Null) return entity;
            
            // Если тега нет (сущность не персонаж), безопасно выходим
            if (!entity.HasComponent<ActiveCharacterTag>())
            {
                UnityEngine.Debug.LogWarning($"[DotsBridge] Попытка Possess для сущности без ActiveCharacterTag!");
                return entity;
            }

            entity.SetEnabled<ActiveCharacterTag>(true);
            UnityEngine.Debug.Log("[DotsBridge] Управление персонажем перехвачено.");
            return entity;
        }

        /// <summary>
        /// Снимает контроль с одиночной сущности.
        /// </summary>
        public static SingleEntity Unpossess(this SingleEntity entity)
        {
            if (entity.Entity == Entity.Null) return entity;
            
            if (entity.HasComponent<ActiveCharacterTag>())
            {
                entity.SetEnabled<ActiveCharacterTag>(false);
                UnityEngine.Debug.Log("[DotsBridge] Управление персонажем отключено.");
            }
            return entity;
        }

        /// <summary>
        /// Массово снимает контроль со всех персонажей в батче.
        /// </summary>
        public static ListEntity UnpossessAll(this ListEntity batch)
        {
            if (!batch.Entities.IsCreated) return batch;
            batch.SetEnabled<ActiveCharacterTag>(false);
            return batch;
        }
    }
}