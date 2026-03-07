using System;
using UnityEngine;
using Unity.Entities;

using Unity.NetCode;
using Unity.Networking.Transport;

namespace DotsBridge.Modules.Network
{
    public class DotsNetworkManager : MonoBehaviour
    {
        public static DotsNetworkManager Instance { get; private set; }

        [Header("Singleton Settings")]
        public bool IsSingleton = true;

        [Header("Connection Settings")]
        public string ServerIP = "127.0.0.1";
        public ushort ServerPort = 7979;

        [Header("Tick Rate Settings")]
        public int SimulationTickRate = 30;
        public int NetworkTickRate = 30;
        public int MaxBatchedTicks = 5;

        [Header("Settings")]
        public bool IsAutoStartServer;

        public static event Action OnClientConnected;
        public static event Action OnServerStarted;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
            Application.runInBackground = true;
            SetupSingleton();
        }
        private void SetupSingleton()
        {
            if (!IsSingleton) return;

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Раскомментируйте, если нужно сохранять между сценами:
            // DontDestroyOnLoad(gameObject); 
        }

        private void Start()
        {

            if (IsAutoStartServer)
                StartServer();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            // Здесь ваша логика очистки реестров
            // EntityBridge.ServerRegistry = null;
            // EntityBridge.ClientRegistry = null;
        }
        internal static void TriggerClientConnected()
        {
            OnClientConnected?.Invoke();
        }
        public void ConnectToServer()
        {
            var clientWorld = GetWorld(WorldFlags.GameClient);
            if (clientWorld == null)
            {
                Debug.LogError("[DotsBridge] Клиентский мир не найден! Убедитесь, что в NetCode Config создан клиент.");
                return;
            }

            if (EntityBridge.ClientRegistry == null || EntityBridge.ClientRegistry.World != clientWorld)
            {
                EntityBridge.ClientRegistry = new BridgeRegistry(clientWorld);
            }

            var em = clientWorld.EntityManager;

            // 1. УБИРАЕМ "МУСОР"
            // Уничтожаем все старые зависшие запросы на коннект, оставшиеся от автоматических попыток Unity
            var pendingRequests = em.CreateEntityQuery(typeof(NetworkStreamRequestConnect));
            em.DestroyEntity(pendingRequests);

            // 2. БЕЗОПАСНЫЙ ПАРСИНГ IP
            ServerIP = ServerIP.Trim(); // Убираем случайные пробелы из UI
            NetworkEndpoint endpoint;

            if (ServerIP == "127.0.0.1" || ServerIP.ToLower() == "localhost")
            {
                endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(ServerPort);
            }
            else if (!NetworkEndpoint.TryParse(ServerIP, ServerPort, out endpoint))
            {
                Debug.LogError($"[DotsBridge] Ошибка: Неверный формат IP адреса: '{ServerIP}'");
                return;
            }

            // 3. ОТПРАВЛЯЕМ ЧИСТЫЙ ЗАПРОС
            var requestEntity = em.CreateEntity(typeof(NetworkStreamRequestConnect));
            em.SetComponentData(requestEntity, new NetworkStreamRequestConnect { Endpoint = endpoint });

            Debug.Log($"[DotsBridge] ОТПРАВЛЕН чистый запрос на подключение к {endpoint.Address}:{ServerPort}");
        }

        public void StartServer()
        {
            var serverWorld = GetWorld(WorldFlags.GameServer);
            if (serverWorld == null)
            {
                Debug.LogError("[DotsBridge] Серверный мир не найден!");
                return;
            }

            if (EntityBridge.ServerRegistry == null || EntityBridge.ServerRegistry.World != serverWorld)
            {
                EntityBridge.ServerRegistry = new BridgeRegistry(serverWorld);
            }

            var em = serverWorld.EntityManager;

            // Защита от двойного клика по кнопке Start Server
            var pendingRequests = em.CreateEntityQuery(typeof(NetworkStreamRequestListen));
            em.DestroyEntity(pendingRequests);

            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(ServerPort);
            var requestEntity = em.CreateEntity(typeof(NetworkStreamRequestListen));
            em.SetComponentData(requestEntity, new NetworkStreamRequestListen { Endpoint = endpoint });

            Debug.Log($"[DotsBridge] СЕРВЕР начинает слушать порт {ServerPort}");
            OnServerStarted?.Invoke();
        }

        private World GetWorld(WorldFlags flag)
        {
            foreach (var world in World.All)
            {
                if (world.IsCreated && world.Flags.HasFlag(flag)) return world;
            }
            return null;
        }
    }
}