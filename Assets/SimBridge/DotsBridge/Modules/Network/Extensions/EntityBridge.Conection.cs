using System;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        public static event Action<SingleEntity, int> OnClientConnected;
        public static event Action<int> OnClientDisconnected;

        internal static void InvokeClientConnected(SingleEntity connection, int networkId)
        {
            OnClientConnected?.Invoke(connection, networkId);
        }

        internal static void InvokeClientDisconnected(int networkId)
        {
            OnClientDisconnected?.Invoke(networkId);
        }

        public static void SubscribeOnClientConnected(Action<SingleEntity, int> onConnected)
        {
            // События статичны, поэтому просто подписываемся. 
            // Проверка на наличие мира полезна, если мы хотим убедиться, что сервер запущен.
            OnClientConnected += onConnected;
        }

        public static void SubscribeOnClientDisconnected(Action<int> onDisconnected)
        {
            OnClientDisconnected += onDisconnected;
        }
    }
}