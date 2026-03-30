using System;
using System.Collections.Generic;
using Unity.Scenes;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;

namespace DotsBridge.Core
{[DefaultExecutionOrder(-100)]
    public class MonoBehaviourBridge : MonoBehaviour
    {
        public static MonoBehaviourBridge Instance { get; private set; }

        [HideLabel, TabGroup("Network")] public NetworkConfig Network = new NetworkConfig();
        [HideLabel, TabGroup("Security")] public SecurityConfig Security = new SecurityConfig();
        [HideLabel, TabGroup("Scenes")] public SceneConfig Scenes = new SceneConfig();

        // Кэш для авторизации (Этап 2)
        [HideInInspector] public string ActiveClientNickname;
        [HideInInspector] public string ActiveClientPassword;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyHeadlessDetection();
            ApplyFramerateSettings();
        }

        private void Start()
        {
            if (Network.IsAutoStart) InitializeNetwork();
        }

        private void OnDestroy() => ClearWorlds();

        private void ApplyHeadlessDetection()
        {
            if (Network.AutoDetectHeadless && (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null || Application.isBatchMode))
            {
                Network.CurrentRole = Role.Server;
                Debug.Log("[DotsBridge] Headless режим. Запуск выделенного сервера.");
            }
        }

        private void ApplyFramerateSettings()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = Network.LimitFPS ? Network.TargetFPS : -1;
            Application.runInBackground = Network.RunInBackground;
        }

        [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f), TabGroup("Network")]
        public void InitializeNetwork()
        {
            if (Network.ClearWorldsOnStart) ClearWorlds();

            Debug.Log($"[DotsBridge] Инициализация в режиме: {Network.CurrentRole}");

            switch (Network.CurrentRole)
            {
                case Role.Server: StartServer(); break;
                case Role.Client: StartClient(); break;
                case Role.ServerClient: StartHost(); break;
            }
        }

        public void ClearWorlds()
        {
            ServerBridge.Dispose();
            ClientBridge.Dispose();

            var worldsToDispose = new List<World>();
            foreach (var world in World.All)
            {
                // Ищем по Contains, потому что Unity может менять точное имя
                if (world.IsClient() || world.IsServer() || world.Name.Contains("Default")) 
                {
                    worldsToDispose.Add(world);
                }
            }
            foreach (var world in worldsToDispose) world.Dispose();
        }

        private void StartServer()
        {
            var world = WorldBuilder.CreateServer(Network, Scenes);
            NetcodeConnector.Listen(world, Network.ServerPort);
            ServerBridge.Initialize(world); // Инициализируем наш API мост
        }

        private void StartClient()
        {
            ActiveClientNickname = Security.DefaultNickname;
            ActiveClientPassword = Security.DefaultPassword;

            string ip = Network.UseLocalhost ? "127.0.0.1" : Network.TargetServerIP;
            
            var world = WorldBuilder.CreateClient(Network, Scenes);
            NetcodeConnector.Connect(world, ip, Network.ServerPort);
            ClientBridge.Initialize(world); // Инициализируем наш API мост
        }

        private void StartHost()
        {
            ActiveClientNickname = "Host_" + Security.DefaultNickname;
            ActiveClientPassword = Security.ServerPassword;

            // Сначала Сервер
            var serverWorld = WorldBuilder.CreateServer(Network, Scenes);
            NetcodeConnector.Listen(serverWorld, Network.ServerPort);
            ServerBridge.Initialize(serverWorld);

            // Затем Клиент (подключается на localhost)
            var clientWorld = WorldBuilder.CreateClient(Network, Scenes);
            NetcodeConnector.Connect(clientWorld, "127.0.0.1", Network.ServerPort);
            ClientBridge.Initialize(clientWorld);
        }
        
        // Метод для вызова из UI (ConnectionBuilder)
        public void StartClientFromUI(string ip, ushort port, string nickname, string password)
        {
            if (Network.ClearWorldsOnStart) ClearWorlds();

            Network.CurrentRole = Role.Client;
            ActiveClientNickname = nickname;
            ActiveClientPassword = password;

            var world = WorldBuilder.CreateClient(Network, Scenes);
            NetcodeConnector.Connect(world, ip, port);
            ClientBridge.Initialize(world);
        }
    }
    public enum Role { ServerClient = 0, Server = 1, Client = 2 }
    public enum SceneLoadMode { None, SingleShared, Separated }

    [Serializable]
    public class NetworkConfig
    {
        [EnumToggleButtons] public Role CurrentRole = Role.ServerClient;
        public bool IsAutoStart = true;
        public bool ClearWorldsOnStart = true;
        public bool AutoDetectHeadless = true;
        
        [Title("Performance")]
        public bool LimitFPS = true;
        [ShowIf("LimitFPS")] public int TargetFPS = 60;
        public bool RunInBackground = true;
        
        [Title("Tick Rate")]
        public int SimulationTickRate = 60;
        public int NetworkTickRate = 60;
        public int MaxBatchedTicks = 5;

        [Title("Connection")]
        public bool UseLocalhost = true;
        [HideIf("UseLocalhost")] public string TargetServerIP = "127.0.0.1";
        public ushort ServerPort = 7979;
    }

    [Serializable]
    public class SecurityConfig
    {
        public bool AutoAcceptConnections = true;
        public string ServerPassword = "";
        public int MaxPlayers = 64;
        
        [Title("Default Client Profile")]
        public string DefaultNickname = "Player_" + 1;
        public string DefaultPassword = "";
    }

    [Serializable]
    public class SceneConfig
    {
        [EnumToggleButtons] public SceneLoadMode LoadMode = SceneLoadMode.Separated;

        [Title("Worlds Content")]
        [Tooltip("Общая сцена (загружается и на Сервер, и на Клиент). Идеально для статичной карты и расставленных вручную стартовых объектов.")]
        public SubScene SharedSubScene; 

        [Tooltip("Только физика и серверная логика (спавнеры, невидимые триггеры, ИИ)")]
        public SubScene ServerSubScene; 
        
        [Tooltip("Только визуал, свет, UI и пост-эффекты")]
        public SubScene ClientSubScene; 

        [Title("Global Prefabs (Network)")]
        [InfoBox("Добавь сюда префабы труб и заводов, которые игрок будет строить динамически во время игры.")]
        public List<GameObject> NetworkPrefabs = new List<GameObject>();
    }
    public static class WorldBuilder
    {
        public static World CreateServer(NetworkConfig netConfig, SceneConfig sceneConfig)
        {
            var world = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            ApplyTickRate(world, netConfig);
            LoadScenes(world, sceneConfig, isServer: true);
            World.DefaultGameObjectInjectionWorld = world;
            return world;
        }

        public static World CreateClient(NetworkConfig netConfig, SceneConfig sceneConfig)
        {
            var world = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            ApplyTickRate(world, netConfig);
            LoadScenes(world, sceneConfig, isServer: false);
            World.DefaultGameObjectInjectionWorld = world;
            return world;
        }

        private static void ApplyTickRate(World world, NetworkConfig config)
        {
            if (world == null || !world.IsCreated) return;

            var em = world.EntityManager;
            var tickRateSettings = new ClientServerTickRate
            {
                SimulationTickRate = config.SimulationTickRate,
                NetworkTickRate = config.NetworkTickRate,
                MaxSimulationStepsPerFrame = (byte)config.MaxBatchedTicks
            };

            using var query = em.CreateEntityQuery(typeof(ClientServerTickRate));
            if (!query.IsEmptyIgnoreFilter) em.DestroyEntity(query);

            var entity = em.CreateEntity(typeof(ClientServerTickRate));
            em.SetComponentData(entity, tickRateSettings);
        }
        private static void LoadScenes(World world, SceneConfig config, bool isServer)
        {
            if (world == null || !world.IsCreated || config.LoadMode == SceneLoadMode.None) return;

            // 1. КРИТИЧЕСКИ ВАЖНО: Shared-сцена должна грузиться ВЕЗДЕ.
            // Именно отсюда Клиент узнает хеши (Ghost Prefab Hashes) для всех сетевых объектов!
            if (config.SharedSubScene != null)
            {
                SceneSystem.LoadSceneAsync(world.Unmanaged, config.SharedSubScene.SceneGUID);
                Debug.Log($"[DotsBridge] Мир {world.Name} загрузил общую сцену (SharedSubScene).");
            }
            
            // 2. Затем загружаем специфичные сцены (физику серверу, графику клиенту)
            if (isServer && config.ServerSubScene != null)
            {
                SceneSystem.LoadSceneAsync(world.Unmanaged, config.ServerSubScene.SceneGUID);
                Debug.Log($"[DotsBridge] СЕРВЕР загрузил свою логику.");
            }
            else if (!isServer && config.ClientSubScene != null)
            {
                SceneSystem.LoadSceneAsync(world.Unmanaged, config.ClientSubScene.SceneGUID);
                Debug.Log($"[DotsBridge] КЛИЕНТ загрузил свой локальный визуал.");
            }
        }
    }
    public static class NetcodeConnector
    {
        public static void Listen(World serverWorld, ushort port)
        {
            var ep = NetworkEndpoint.AnyIpv4.WithPort(port);
            var em = serverWorld.EntityManager;
            var listenEntity = em.CreateEntity(typeof(NetworkStreamRequestListen));
            em.SetComponentData(listenEntity, new NetworkStreamRequestListen { Endpoint = ep });
        }

        public static void Connect(World clientWorld, string ip, ushort port)
        {
            var ep = NetworkEndpoint.Parse(ip, port);
            var em = clientWorld.EntityManager;
            var connectEntity = em.CreateEntity(typeof(NetworkStreamRequestConnect));
            em.SetComponentData(connectEntity, new NetworkStreamRequestConnect { Endpoint = ep });
        }
    }
}