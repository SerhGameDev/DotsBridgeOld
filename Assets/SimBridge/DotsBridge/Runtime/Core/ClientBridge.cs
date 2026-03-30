using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.NetCode;

namespace DotsBridge
{
    public static class ClientBridge
    {
        public static World NativeWorld { get; private set; }
        
        public static EntityManager Manager => NativeWorld.EntityManager;
        
        public static bool IsActive => NativeWorld != null && NativeWorld.IsCreated;

        public static event Action OnStarted;
        public static event Action OnStopped;

        internal static void Initialize(World world)
        {
            NativeWorld = world;
            OnStarted?.Invoke();
            UnityEngine.Debug.Log("[ClientBridge] API Клиента готово к работе.");
        }

        internal static void Dispose()
        {
            if (IsActive)
            {
                OnStopped?.Invoke();
            }
            NativeWorld = null;
            UnityEngine.Debug.Log("[ClientBridge] API Клиента отключено.");
        }

        /// <summary>
        /// Возвращает обертку BridgeWorld для работы с твоими батчами (ListEntity).
        /// Использование: ClientBridge.World().FindWithComponent<T>()
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BridgeWorld World()
        {
            return new BridgeWorld(NativeWorld);
        }
        
        /// <summary>
        /// Отправляет гарантированный запрос (RPC) на сервер (например, "Хочу построить завод").
        /// </summary>
        public static void SendToServer<T>(T message) where T : unmanaged, IRpcCommand
        {
            if (!IsActive) return;

            var entityManager = Manager;
            
            // Ищем наше активное подключение к серверу
            using var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<NetworkStreamConnection>());
            
            if (query.IsEmptyIgnoreFilter)
            {
                UnityEngine.Debug.LogWarning("[ClientBridge] Нет активного подключения к серверу для отправки сообщения.");
                return;
            }

            var connectionEntity = query.GetSingletonEntity();

            var rpcEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(rpcEntity, message);
            entityManager.AddComponentData(rpcEntity, new SendRpcCommandRequest { TargetConnection = connectionEntity });
        }
    }
}