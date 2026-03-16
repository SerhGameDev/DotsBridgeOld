using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

namespace DotsBridge.Modules.Network
{
    public enum Role
    {
        ServerClient = 0, // Host
        Server = 1,       // Dedicated Server
        Client = 2,       // Pure Client
    }

    /// <summary>
    /// Статическое ядро для управления сетевыми мирами DOTS.
    /// </summary>
    public static class DotsNetworkManager
    {
        public static Role CurrentRole { get; private set; }

        // События. Передаем World, чтобы подписчики сразу могли получить к нему доступ.


        /// <summary>
        /// Создает серверный мир и начинает прослушивание порта.
        /// </summary>
        public static void StartServer(ushort port)
        {
            CurrentRole = Role.Server;
            DestroyDefaultWorld();

            var serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            World.DefaultGameObjectInjectionWorld = serverWorld;

            using var query = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(NetworkEndpoint.AnyIpv4.WithPort(port));

            Debug.Log($"[DotsNetworkManager] Сервер запущен на порту {port}");
        }

        /// <summary>
        /// Создает клиентский мир и инициирует подключение к серверу.
        /// </summary>
        public static void ConnectClient(string ip, ushort port)
        {
            if (CurrentRole != Role.ServerClient)
                CurrentRole = Role.Client;

            if (CurrentRole == Role.Client)
                DestroyDefaultWorld();

            var clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");

            if (CurrentRole == Role.Client)
                World.DefaultGameObjectInjectionWorld = clientWorld;

            var endpoint = NetworkEndpoint.Parse(ip, port);

            using var query = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, endpoint);

            Debug.Log($"[DotsNetworkManager] Клиент инициировал подключение к {ip}:{port}");
        }

        /// <summary>
        /// Запускает и сервер, и клиента (режим Host).
        /// </summary>
        public static void StartHost(string ip, ushort port)
        {
            CurrentRole = Role.ServerClient;
            StartServer(port);
            ConnectClient(ip, port);
        }

        /// <summary>
        /// Уничтожает стандартный пустой мир, созданный Unity при запуске.
        /// </summary>
        private static void DestroyDefaultWorld()
        {
            foreach (var world in World.All)
            {
                if (world.Flags == WorldFlags.Game)
                {
                    world.Dispose();
                    break;
                }
            }
        }
    }
}