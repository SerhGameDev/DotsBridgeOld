using System;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Гарантирует, что система обработки и очистки RPC типа T существует во всех активных мирах.
        /// </summary>
        private static void EnsureRpcSystemExists<T>() where T : unmanaged, IRpcCommand
        {
            foreach (var world in World.All)
            {
                if ((world.Flags & (WorldFlags.GameClient | WorldFlags.GameServer)) != 0)
                {
                    var sys = world.GetExistingSystemManaged<GenericRpcSystem<T>>();
                    if (sys == null)
                    {
                        sys = world.CreateSystemManaged<GenericRpcSystem<T>>();

                        var group = world.GetExistingSystemManaged<SimulationSystemGroup>();
                        group.AddSystemToUpdateList(sys);

                        group.SortSystems();
                    }
                }
            }
        }

        public static void SubscribeRpc<T>(this BridgeWorld bridge, Action<T, SingleEntity> onReceive) where T : unmanaged, IRpcCommand
        {
            if (bridge == null)
            {
                return;
            }

            EnsureRpcSystemExists<T>();

            var sysHandle = bridge.World.GetExistingSystemManaged<GenericRpcSystem<T>>();
            if (sysHandle != null)
            {
                sysHandle.OnReceived += onReceive;
            }
        }
        public static void SendRpc<T>(this BridgeWorld bridge, T command) where T : unmanaged, IRpcCommand
        {
            if (bridge == null) return;
            EnsureRpcSystemExists<T>();
            var rpcEntity = bridge.Manager.CreateEntity();
            bridge.Manager.AddComponentData(rpcEntity, command);

            if (bridge.World.IsClient())
            {
                var targetConn = GetServerConnection(bridge);
                if (targetConn == Entity.Null)
                {
                    Debug.LogError($"[DotsBridge] Ошибка Клиента: Нет соединения с Сервером для отправки {typeof(T).Name}");
                    return;
                }

                bridge.Manager.AddComponentData(rpcEntity, new SendRpcCommandRequest { TargetConnection = targetConn });
            }
            else if (bridge.World.IsServer())
            {
                bridge.Manager.AddComponentData(rpcEntity, new SendRpcCommandRequest());
            }
        }
        /// <summary>
        /// Отписаться от RPC (важно делать при уничтожении объектов, чтобы избежать утечек).
        /// </summary>
        public static void UnsubscribeRpc<T>(this BridgeWorld bridge, Action<T, SingleEntity> onReceive) where T : unmanaged, IRpcCommand
        {
            if (bridge == null || !bridge.World.IsCreated) return;

            var sysHandle = bridge.World.GetExistingSystemManaged<GenericRpcSystem<T>>();
            if (sysHandle != null)
            {
                sysHandle.OnReceived -= onReceive;
            }
        }

        /// <summary>
        /// Отправляет RPC конкретному клиенту по его NetworkId (вызывается только на Сервере).
        /// </summary>
        public static void SendRpcToClient<T>(this BridgeWorld bridge, T command, int targetNetworkId) where T : unmanaged, IRpcCommand
        {
            if (bridge == null || !bridge.World.IsServer()) return;

            EnsureRpcSystemExists<T>();

            // Ищем все активные соединения на сервере
            var query = bridge.Manager.CreateEntityQuery(typeof(NetworkId));
            if (query.IsEmptyIgnoreFilter) return;

            using var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            using var netIds = query.ToComponentDataArray<NetworkId>(Unity.Collections.Allocator.Temp);

            Entity targetConnection = Entity.Null;

            // Находим сущность соединения с нужным NetworkId
            for (int i = 0; i < entities.Length; i++)
            {
                if (netIds[i].Value == targetNetworkId)
                {
                    targetConnection = entities[i];
                    break;
                }
            }

            if (targetConnection != Entity.Null)
            {
                var rpcEntity = bridge.Manager.CreateEntity();
                bridge.Manager.AddComponentData(rpcEntity, command);
                // Явно указываем получателя
                bridge.Manager.AddComponentData(rpcEntity, new SendRpcCommandRequest { TargetConnection = targetConnection });
            }
            else
            {
                Debug.LogWarning($"[DotsBridge] Ошибка отправки: Клиент с NetworkId {targetNetworkId} не найден.");
            }
        }

        /// <summary>
        /// Рассылает RPC всем подключенным клиентам (вызывается только на Сервере).
        /// </summary>
        public static void BroadcastRpc<T>(this BridgeWorld bridge, T command) where T : unmanaged, IRpcCommand
        {
            if (bridge == null || !bridge.World.IsServer()) return;

            EnsureRpcSystemExists<T>();

            var rpcEntity = bridge.Manager.CreateEntity();
            bridge.Manager.AddComponentData(rpcEntity, command);

            // В Unity NetCode пустой SendRpcCommandRequest на сервере автоматически делает Broadcast всем клиентам
            bridge.Manager.AddComponentData(rpcEntity, new SendRpcCommandRequest());
        }
    }
}