#if DOTSBRIDGE_NETCODE
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge.Modules.Network
{
    // 1. Сама структура RPC (то, что полетит по сети)
    public struct OopEventRpc : IRpcCommand
    {
        public int EventHash;
        public int IntValue;
        public float FloatValue;
        public float3 VectorValue;
    }

    // 2. Реестр для подписки из ООП (Словари живут здесь)
    public static class OopRpcRegistry
    {
        // Словари для сервера и клиента
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

        // Внутренние методы для систем
        internal static void InvokeOnServer(int hash, OopEventRpc data)
        {
            if (_serverListeners.TryGetValue(hash, out var action)) action?.Invoke(data);
        }

        internal static void InvokeOnClient(int hash, OopEventRpc data)
        {
            if (_clientListeners.TryGetValue(hash, out var action)) action?.Invoke(data);
        }
    }
}
#endif