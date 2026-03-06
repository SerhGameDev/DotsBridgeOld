#if DOTSBRIDGE_NETCODE
using DotsBridge.Modules.Network;
using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

namespace DotsBridge
{
    public static partial class NetEntityBridge
    {
        // --- ГЛОБАЛЬНЫЕ СЕТЕВЫЕ СОБЫТИЯ ---
        public static event Action OnClientConnected;
        public static event Action OnServerStarted;

        internal static void TriggerClientConnected() => OnClientConnected?.Invoke();
        internal static void TriggerServerStarted() => OnServerStarted?.Invoke();

        // =========================================================
        // УПРАВЛЕНИЕ ПОДКЛЮЧЕНИЕМ
        // =========================================================
        public static void ConnectToServer(string ip, ushort port, int simTickRate = 60, int netTickRate = 60)
        {
            var clientWorld = GetWorld(WorldFlags.GameClient);
            if (clientWorld == null)
            {
                Debug.LogError("[DotsBridge] Клиентский мир не найден.");
                return;
            }

            var em = clientWorld.EntityManager;

            if (!em.CreateEntityQuery(typeof(NetworkStreamRequestConnect)).IsEmptyIgnoreFilter ||
                !em.CreateEntityQuery(typeof(NetworkId)).IsEmptyIgnoreFilter)
            {
                Debug.LogWarning("[DotsBridge] Клиент уже подключается или подключен!");
                return;
            }

            ApplyNetworkSettings(clientWorld, simTickRate, netTickRate);

            var endpoint = NetworkEndpoint.Parse(ip, port);
            var entity = em.CreateEntity(typeof(NetworkStreamRequestConnect));
            em.SetComponentData(entity, new NetworkStreamRequestConnect { Endpoint = endpoint });

            Debug.Log($"[DotsBridge] Отправка запроса на подключение к {ip}:{port}...");
        }

        public static void StartServer(ushort port, int simTickRate = 60, int netTickRate = 60)
        {
            var serverWorld = GetWorld(WorldFlags.GameServer);
            if (serverWorld == null) return;

            var em = serverWorld.EntityManager;

            if (!em.CreateEntityQuery(typeof(NetworkStreamRequestListen)).IsEmptyIgnoreFilter)
            {
                Debug.LogWarning("[DotsBridge] Сервер уже запущен!");
                return;
            }

            ApplyNetworkSettings(serverWorld, simTickRate, netTickRate);

            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(port);
            var entity = em.CreateEntity(typeof(NetworkStreamRequestListen));
            em.SetComponentData(entity, new NetworkStreamRequestListen { Endpoint = endpoint });

            Debug.Log($"[DotsBridge] Сервер запущен на порту {port}.");
            TriggerServerStarted();
        }

        // =========================================================
        // ВНУТРЕННИЕ УТИЛИТЫ
        // =========================================================
        private static World GetWorld(WorldFlags flag)
        {
            foreach (var world in World.All)
            {
                if (world.IsCreated && world.Flags.HasFlag(flag)) return world;
            }
            return null;
        }

        private static void ApplyNetworkSettings(World world, int simTickRate, int netTickRate)
        {
            var em = world.EntityManager;
            var tickRateData = new ClientServerTickRate
            {
                SimulationTickRate = simTickRate,
                NetworkTickRate = netTickRate
            };

            var query = em.CreateEntityQuery(typeof(ClientServerTickRate));
            if (query.HasSingleton<ClientServerTickRate>())
            {
                query.SetSingleton(tickRateData);
            }
            else
            {
                var entity = em.CreateEntity(typeof(ClientServerTickRate));
                em.SetComponentData(entity, tickRateData);
            }
        }

        /// <summary>
        /// Подключает клиента к серверу, используя настройки из DotsNetworkManager.Instance на сцене.
        /// </summary>
        public static void ConnectToServer()
        {
            var config = DotsNetworkManager.Instance;
            if (config == null)
            {
                Debug.LogError("[DotsBridge] ОШИБКА: DotsNetworkManager.Instance не найден! Либо добавьте его на сцену и включите UseAsSingleton, либо используйте метод ConnectToServer(ip, port).");
                return;
            }

            ConnectToServer(config.ServerIP, config.ServerPort, config.SimulationTickRate, config.NetworkTickRate);
        }

        /// <summary>
        /// Запускает сервер, используя настройки из DotsNetworkManager.Instance на сцене.
        /// </summary>
        public static void StartServer()
        {
            var config = DotsNetworkManager.Instance;
            if (config == null)
            {
                Debug.LogError("[DotsBridge] ОШИБКА: DotsNetworkManager.Instance не найден! Либо добавьте его на сцену и включите UseAsSingleton, либо используйте метод StartServer(port).");
                return;
            }

            StartServer(config.ServerPort, config.SimulationTickRate, config.NetworkTickRate);
        }
    }
}
#endif