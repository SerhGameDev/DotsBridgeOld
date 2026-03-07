using System;
using System.Collections.Generic;
using Unity.NetCode;
using Unity.Mathematics;

namespace DotsBridge.Modules.Network
{
    public static class OopRpcRegistry
    {
        private static readonly Dictionary<int, Action<OopEventRpc>> _serverListeners = new();
        private static readonly Dictionary<int, Action<OopEventRpc>> _clientListeners = new();

        /// <summary>
        /// Подписаться на событие, пришедшее на СЕРВЕР (от клиента).
        /// </summary>
        public static void SubscribeOnServer(string eventName, Action<OopEventRpc> callback)
        {
            int hash = EntityBridge.GetHash(eventName);
            if (!_serverListeners.TryAdd(hash, callback))
                _serverListeners[hash] += callback;
        }

        /// <summary>
        /// Подписаться на событие, пришедшее на КЛИЕНТ (от сервера).
        /// </summary>
        public static void SubscribeOnClient(string eventName, Action<OopEventRpc> callback)
        {
            int hash = EntityBridge.GetHash(eventName);
            if (!_clientListeners.TryAdd(hash, callback))
                _clientListeners[hash] += callback;
        }

        public static void UnsubscribeOnServer(string eventName, Action<OopEventRpc> callback)
        {
            int hash = EntityBridge.GetHash(eventName);
            if (_serverListeners.ContainsKey(hash)) _serverListeners[hash] -= callback;
        }

        public static void UnsubscribeOnClient(string eventName, Action<OopEventRpc> callback)
        {
            int hash = EntityBridge.GetHash(eventName);
            if (_clientListeners.ContainsKey(hash)) _clientListeners[hash] -= callback;
        }

        internal static void InvokeOnServer(int hash, OopEventRpc data)
        {
            if (_serverListeners.TryGetValue(hash, out var action)) action?.Invoke(data);
        }

        internal static void InvokeOnClient(int hash, OopEventRpc data)
        {
            if (_clientListeners.TryGetValue(hash, out var action)) action?.Invoke(data);
        }
        
        /// <summary>
         /// Отправить RPC на СЕРВЕР (вызывать с Клиента).
         /// </summary>
        public static void SendToServer(string eventName, int intVal = 0, float floatVal = 0, float3 vecVal = default)
        {
            var registry = EntityBridge.ClientRegistry;
            if (registry == null) return;

            var em = registry.Manager;
            var rpcData = new OopEventRpc
            {
                EventHash = EntityBridge.GetHash(eventName),
                IntValue = intVal,
                FloatValue = floatVal,
                VectorValue = vecVal
            };

            var reqEntity = em.CreateEntity(typeof(OopEventRpc), typeof(SendRpcCommandRequest));
            em.SetComponentData(reqEntity, rpcData);
        }
    }
}