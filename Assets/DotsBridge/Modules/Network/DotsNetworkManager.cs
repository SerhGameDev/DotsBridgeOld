using System;
using UnityEngine;
using Unity.Entities;

#if DOTSBRIDGE_NETCODE
using Unity.NetCode;
using Unity.Networking.Transport;
#endif

namespace DotsBridge.Modules.Network
{
    public class DotsNetworkManager : MonoBehaviour
    {
        [Header("Connection Settings")]
        public string ServerIP = "127.0.0.1";
        public ushort ServerPort = 7979;

        [Header("Tick Rate Settings")]
        [Tooltip("Частота обновления физики и логики в секунду (по умолчанию 60)")]
        public int SimulationTickRate = 60;

        [Tooltip("Как часто сервер отправляет пакеты клиентам в секунду (по умолчанию 60)")]
        public int NetworkTickRate = 60;

        [Tooltip("Максимальное количество тиков за кадр при лагах (чтобы догнать сервер)")]
        public int MaxBatchedTicks = 3;

        // --- СОБЫТИЯ ---
        public static event Action OnClientConnected;
        public static event Action OnServerStarted;

        private void Awake()
        {
            // Чтобы ParrelSync работал и игра не засыпала без фокуса
            Application.runInBackground = true;
        }

        // --- МЕТОДЫ ДЛЯ UI И OOP ---
        public void ConnectToServer()
        {
#if DOTSBRIDGE_NETCODE
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

            ApplyNetworkSettings(clientWorld); // Применяем настройки из инспектора

            var endpoint = NetworkEndpoint.Parse(ServerIP, ServerPort);
            var entity = em.CreateEntity(typeof(NetworkStreamRequestConnect));
            em.SetComponentData(entity, new NetworkStreamRequestConnect { Endpoint = endpoint });

            Debug.Log($"[DotsBridge] Отправка запроса на подключение к {ServerIP}:{ServerPort}...");
#endif
        }

        public void StartServer()
        {
#if DOTSBRIDGE_NETCODE
            var serverWorld = GetWorld(WorldFlags.GameServer);
            if (serverWorld == null) return;

            var em = serverWorld.EntityManager;

            if (!em.CreateEntityQuery(typeof(NetworkStreamRequestListen)).IsEmptyIgnoreFilter)
            {
                Debug.LogWarning("[DotsBridge] Сервер уже запущен!");
                return;
            }

            ApplyNetworkSettings(serverWorld); // Применяем настройки из инспектора

            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(ServerPort);
            var entity = em.CreateEntity(typeof(NetworkStreamRequestListen));
            em.SetComponentData(entity, new NetworkStreamRequestListen { Endpoint = endpoint });

            Debug.Log($"[DotsBridge] Сервер запущен на порту {ServerPort}.");
            OnServerStarted?.Invoke();
#endif
        }

        internal static void TriggerClientConnected()
        {
            OnClientConnected?.Invoke();
        }

        private World GetWorld(WorldFlags flag)
        {
            foreach (var world in World.All)
            {
                if (world.IsCreated && world.Flags.HasFlag(flag)) return world;
            }
            return null;
        }
#if DOTSBRIDGE_NETCODE
        private void ApplyNetworkSettings(World world)
        {
            var em = world.EntityManager;

            // Настраиваем только то, что есть в актуальной версии Netcode
            var tickRateData = new ClientServerTickRate
            {
                SimulationTickRate = this.SimulationTickRate,
                NetworkTickRate = this.NetworkTickRate
            };

            // В новых версиях Entities работа с синглтонами идет через Query
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
#endif
    }
}