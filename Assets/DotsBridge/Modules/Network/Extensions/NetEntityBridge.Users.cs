#if DOTSBRIDGE_NETCODE
using DotsBridge.Modules.Network;
using System;
using System.Collections.Generic;
using Unity.NetCode;
using UnityEngine;

namespace DotsBridge
{
    public static partial class NetEntityBridge
    {
        // =========================================================
        // РЕЕСТР ПОЛЬЗОВАТЕЛЕЙ
        // =========================================================
        public static readonly HashSet<int> ConnectedUsers = new HashSet<int>();

        public static event Action<int> OnUserConnected;
        public static event Action<int> OnUserDisconnected;

        internal static void AddUser(int networkId)
        {
            if (ConnectedUsers.Add(networkId))
            {
                Debug.Log($"[DotsBridge] Пользователь {networkId} подключился. Всего игроков: {ConnectedUsers.Count}");
                OnUserConnected?.Invoke(networkId);
            }
        }

        internal static void RemoveUser(int networkId)
        {
            if (ConnectedUsers.Remove(networkId))
            {
                Debug.Log($"[DotsBridge] Пользователь {networkId} отключился. Всего игроков: {ConnectedUsers.Count}");
                OnUserDisconnected?.Invoke(networkId);
            }
        }

    }
    public static partial class EntityBridge
    {
        /// <summary>
        /// Назначает владельца (NetworkId) для всех заспавненных сущностей в батче.
        /// </summary>
        public static EntityBatch SetOwner(this EntityBatch batch, int networkId)
        {
            if (!batch.Entities.IsCreated || batch.Entities.Length == 0) return batch;

            var em = batch.Manager;

            foreach (var entity in batch.Entities)
            {
                em.AddComponentData(entity, new UserOwner { NetworkId = networkId });

                if (em.HasComponent<GhostOwner>(entity))
                {
                    em.SetComponentData(entity, new GhostOwner { NetworkId = networkId });
                }
            }
            return batch;
        }

        public static DotsCommand SetOwner(this DotsCommand сommand, int networkId)
        {
            return сommand.Do(batch => batch.SetOwner(networkId)); ;
        }
    }
}
#endif