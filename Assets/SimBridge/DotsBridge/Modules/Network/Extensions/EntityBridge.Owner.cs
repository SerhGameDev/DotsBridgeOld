using DotsBridge;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode; 
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Безопасно получает NetworkId из сущности. Возвращает -1, если компонента нет.
        /// </summary>
        public static int GetNetworkId(this SingleEntity entity)
        {
            if (entity.Manager == default || !entity.Manager.Exists(entity.Entity))
                return -1;

            return entity.HasComponent<NetworkId>()
                ? entity.GetComponent<NetworkId>().Value
                : -1;
        }

        /// <summary>
        /// Автоматически находит NetworkId текущего клиента.
        /// Безопасно вернет -1, если вызвать на сервере или до подключения.
        /// </summary>
        public static int GetLocalNetworkId(BridgeWorld bridge)
        {
            // Защита: на сервере может быть много NetworkId, автоопределение невозможно
            if ((bridge.World.Flags & WorldFlags.GameClient) == 0)
            {
                Debug.LogWarning("[DotsBridge] Попытка получить Local NetworkId на сервере! Используйте явную передачу ID.");
                return -1;
            }

            var query = bridge.Manager.CreateEntityQuery(typeof(NetworkId));
            if (query.IsEmptyIgnoreFilter) return -1;

            // В клиенте всегда только одно подключение к серверу
            return query.GetSingleton<NetworkId>().Value;
        }

        // =========================================================
        // РАСШИРЕНИЯ ДЛЯ ListEntity
        // =========================================================

        /// <summary>
        /// Назначает владельца всем сущностям в батче (Явное указание ID).
        /// </summary>
        public static ListEntity SetOwner(this ListEntity batch, int networkId)
        {
            return batch.AddComponent(new NetworkOwner { Value = networkId });
        }

        /// <summary>
        /// Назначает владельца всем сущностям в батче (Автоопределение локального клиента).
        /// </summary>
        public static ListEntity SetOwner(this ListEntity batch)
        {
            return batch.SetOwner(GetLocalNetworkId(batch.Word));
        }

        /// <summary>
        /// Фильтрует батч, оставляя только те сущности, которые принадлежат указанному NetworkId.
        /// </summary>
        public static ListEntity WithOwner(this ListEntity batch, int networkId)
        {
            return batch.Filter((SingleEntity entity) =>
                entity.HasComponent<NetworkOwner>() &&
                entity.GetComponent<NetworkOwner>().Value == networkId);
        }

        /// <summary>
        /// Фильтрует батч, оставляя только те сущности, которые принадлежат НАМ (локальному клиенту).
        /// </summary>
        public static ListEntity WithOwner(this ListEntity batch)
        {
            return batch.WithOwner(GetLocalNetworkId(batch.Word));
        }

        // =========================================================
        // РАСШИРЕНИЯ ДЛЯ DotsCommand
        // =========================================================

        /// <summary>
        /// Команда: Назначает владельца (Явное указание ID).
        /// </summary>
        public static DotsCommand SetOwner(this DotsCommand cmd, int networkId)
        {
            return cmd.Do(new SetOwnerAction { NetworkId = networkId });
        }

        /// <summary>
        /// Команда: Назначает владельца (Автоопределение).
        /// </summary>
        public static DotsCommand SetOwner(this DotsCommand cmd)
        {
            return cmd.Do(new SetLocalOwnerAction());
        }

        private class SetOwnerAction : ICommandAction
        {
            public int NetworkId;
            public void Execute(ListEntity batch) => batch.SetOwner(NetworkId);
        }

        private class SetLocalOwnerAction : ICommandAction
        {
            // Экшен вызывает ListEntity.SetOwner без параметров (где уже есть GetLocalNetworkId)
            public void Execute(ListEntity batch) => batch.SetOwner();
        }

        /// <summary>
        /// Команда: Фильтрует по владельцу (Явное указание ID).
        /// </summary>
        public static DotsCommand WithOwner(this DotsCommand cmd, int networkId)
        {
            return cmd.Do(new WithOwnerAction { NetworkId = networkId });
        }

        /// <summary>
        /// Команда: Фильтрует по НАМ (локальному клиенту).
        /// </summary>
        public static DotsCommand WithOwner(this DotsCommand cmd)
        {
            return cmd.Do(new WithLocalOwnerAction());
        }

        private class WithOwnerAction : ICommandAction
        {
            public int NetworkId;
            public void Execute(ListEntity batch) => batch.WithOwner(NetworkId);
        }

        private class WithLocalOwnerAction : ICommandAction
        {
            public void Execute(ListEntity batch) => batch.WithOwner();
        }


        public static void DestroyClientEntities(BridgeWorld bridge, int networkId)
        {
            if (bridge == null) return;

            var query = bridge.Manager.CreateEntityQuery(typeof(NetworkOwner));
            if (query.IsEmptyIgnoreFilter) return;

            using var tempEntities = query.ToEntityArray(Allocator.Temp);
            using var batch = new ListEntity(bridge, Allocator.Temp);

            batch.Entities.AddRange(tempEntities);
            batch.WithOwner(networkId).ClearAndDestroy();
        }
    }
}
