using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    public static partial class EntityBridge
    {

        /// <summary>
        /// ID текущего локального игрока. 
        /// В синглплеере оставьте 0. 
        /// В мультиплеере клиент должен записать сюда свой NetworkId.
        /// </summary>
        public static int LocalPlayerId = 0;

        // ==========================================
        // ФИЛЬТРЫ (FILTERS)
        // ==========================================

        /// <summary>
        /// Безопасно фильтрует батч, оставляя только сущности указанного владельца.
        /// Если clientId не указан, использует LocalPlayerId (удобно для клиента/синглплеера).
        /// </summary>
        public static EntityBatch WithOwner(this EntityBatch batch, int clientId = -1)
        {
            if (batch.Manager == default || !batch.Entities.IsCreated) return batch;

            int targetId = clientId == -1 ? LocalPlayerId : clientId;

            // Создаем новый временный список (живет 1 кадр).
            var filtered = new NativeList<Entity>(Allocator.Temp);

            foreach (var entity in batch.Entities)
            {
                if (batch.Manager.HasComponent<BridgeOwner>(entity) &&
                    batch.Manager.GetComponentData<BridgeOwner>(entity).ClientId == targetId)
                {
                    filtered.Add(entity);
                }
            }

            // ИСПРАВЛЕНИЕ: Возвращаем новый батч с отфильтрованным списком,
            // вместо того чтобы пытаться перезаписать readonly поле.
            return new EntityBatch(filtered, batch.Manager);
        }


        /// <summary>
        /// Позволяет назначить владельца уже существующей сущности после Get.
        /// </summary>
        public static EntityBatch SetOwner(this EntityBatch batch, int clientId)
        {
            if (batch.Manager == default) return batch;

            foreach (var entity in batch.Entities)
            {
                if (batch.Manager.HasComponent<BridgeOwner>(entity))
                    batch.Manager.SetComponentData(entity, new BridgeOwner { ClientId = clientId });
                else
                    batch.Manager.AddComponentData(entity, new BridgeOwner { ClientId = clientId });
            }
            return batch;
        }
    }
}