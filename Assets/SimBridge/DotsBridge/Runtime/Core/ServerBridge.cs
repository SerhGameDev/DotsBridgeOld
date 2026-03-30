using System;
using System.Runtime.CompilerServices;
using DotsBridge.Modules.Network;
using Unity.Entities;
using Unity.NetCode;

namespace DotsBridge
{
    public static class ServerBridge
    {
        public static World NativeWorld { get; private set; }
        
        // Быстрый доступ к менеджеру сущностей, если нужен
        public static EntityManager Manager => NativeWorld.EntityManager;
        
        public static bool IsActive => NativeWorld != null && NativeWorld.IsCreated;

        public static event Action OnStarted;
        public static event Action OnStopped;

        internal static void Initialize(World world)
        {
            NativeWorld = world;
            OnStarted?.Invoke();
            UnityEngine.Debug.Log("[ServerBridge] API Сервера готово к работе.");
        }

        internal static void Dispose()
        {
            if (IsActive)
            {
                OnStopped?.Invoke();
            }
            NativeWorld = null;
            UnityEngine.Debug.Log("[ServerBridge] API Сервера отключено.");
        }

        /// <summary>
        /// Возвращает обертку BridgeWorld для работы с твоими батчами (ListEntity).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BridgeWorld World()
        {
            return new BridgeWorld(NativeWorld);
        }
        /// <summary>
        /// Отправляет гарантированное событие (RPC) конкретному клиенту.
        /// </summary>
        public static void SendToClient<T>(int clientNetworkId, T message) where T : unmanaged, IRpcCommand
        {
            if (!IsActive) return;

            if (ConnectionManager.Players.TryGetValue(clientNetworkId, out var session))
            {
                var entityManager = Manager;
                var rpcEntity = entityManager.CreateEntity();
                
                entityManager.AddComponentData(rpcEntity, message);
                entityManager.AddComponentData(rpcEntity, new SendRpcCommandRequest { TargetConnection = session.ConnectionEntity });
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[ServerBridge] Попытка отправить сообщение отключенному клиенту (ID: {clientNetworkId})");
            }
        }

        /// <summary>
        /// Рассылает гарантированное событие (RPC) всем подключенным клиентам (Broadcast).
        /// </summary>
        public static void SendToAllClient<T>(T message) where T : unmanaged, IRpcCommand
        {
            if (!IsActive) return;

            var entityManager = Manager;
            
            foreach (var session in ConnectionManager.Players.Values)
            {
                var rpcEntity = entityManager.CreateEntity();
                entityManager.AddComponentData(rpcEntity, message);
                entityManager.AddComponentData(rpcEntity, new SendRpcCommandRequest { TargetConnection = session.ConnectionEntity });
            }
        }
    }
}