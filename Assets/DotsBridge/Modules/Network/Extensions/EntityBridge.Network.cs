using DotsBridge.Modules.Network;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        private static DotsNetworkManager GetManager()
        {
            if (DotsNetworkManager.Instance != null)
                return DotsNetworkManager.Instance;

            // Если синглтон выключен, пытаемся найти на сцене
            return Object.FindAnyObjectByType<DotsNetworkManager>();
        }

        public static void Connect()
        {
            var manager = GetManager();
            if (manager != null) DotsNetworkManager.Instance.ConnectToServer();
            else Debug.LogError("[DotsBridge] DotsNetworkManager не найден на сцене.");
        }

        public static void StartServer()
        {
            var manager = GetManager();
            if (manager != null) DotsNetworkManager.Instance.StartServer();
            else Debug.LogError("[DotsBridge] DotsNetworkManager не найден на сцене.");
        }
    }
}