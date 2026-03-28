using System;
using System.Collections.Generic;
using DotsBridge;
using DotsBridge.Modules.Network;
using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace DotsBridge
{public enum Role
    {
        ServerClient = 0, // Host
        Server = 1,       // Dedicated Server
        Client = 2,       // Pure Client
    }

    public enum SceneLoadMode
    {
        None,
        SingleShared, 
        Separated     
    }

    [DefaultExecutionOrder(-100)]
    public class DotsBridgeBootstrapper : MonoBehaviour
    {
        public static DotsBridgeBootstrapper Instance { get; private set; }

        #region General Configuration
        [BoxGroup("General Configuration", centerLabel: true)]
        [EnumToggleButtons]
        public Role CurrentRole = Role.ServerClient;
        
        [BoxGroup("General Configuration")]
        [Tooltip("Запускать сеть автоматически при старте сцены")]
        public bool IsAutoStart = true;
        
        [BoxGroup("General Configuration")]
        [Tooltip("Принудительно удаляет старые сетевые миры при старте. Полезно при рестартах в Editor.")]
        public bool ClearWorldsOnStart = true;
        
        [BoxGroup("General Configuration")]
        [Tooltip("Автоматически переключаться на Server, если запущен выделенный сервер без графики (Linux/BatchMode).")]
        public bool AutoDetectHeadless = true;
        #endregion

        #region Performance Configuration
        [BoxGroup("Performance Configuration", centerLabel: true)]
        public bool LimitFPS = true;

        [BoxGroup("Performance Configuration")]
        [ShowIf("LimitFPS")]
        [Tooltip("Ограничение FPS главного потока Unity. Снимает нагрузку с CPU.")]
        public int TargetFPS = 60;
        
        [BoxGroup("Performance Configuration")]
        public bool RunInBackground = true;
        
        [BoxGroup("Performance Configuration")]
        [Tooltip("Настройки Netcode Tick Rate (Симуляция).")]
        public int SimulationTickRate = 60;
        
        [BoxGroup("Performance Configuration")]
        public int NetworkTickRate = 60;
        
        [BoxGroup("Performance Configuration")]
        public int MaxBatchedTicks = 5;
        #endregion

        #region Connection Settings
        [BoxGroup("Connection Settings", centerLabel: true)]
        [InfoBox("LAN IP этого ПК: @GetLocalIPAddress()", InfoMessageType.None)]
        [Tooltip("Использовать 127.0.0.1 для локальных тестов на одной машине.")]
        public bool UseLocalhost = true;

        [BoxGroup("Connection Settings")]
        [HideIf("UseLocalhost")]
        [Tooltip("Укажите IP-адрес ПК в локальной сети (например, 192.168.1.100)")]
        public string TargetServerIP = "127.0.0.1";

        [BoxGroup("Connection Settings")]
        public ushort ServerPort = 7979;
        #endregion

        #region Scene Management
        [BoxGroup("Scene Management", centerLabel: true)]
        [EnumToggleButtons]
        public SceneLoadMode LoadMode = SceneLoadMode.SingleShared;

        [BoxGroup("Scene Management")]
        [ShowIf("LoadMode", SceneLoadMode.SingleShared)]
        [InfoBox("Эта SubScene будет загружена и на Клиент, и на Сервер.", InfoMessageType.None)]
        public SubScene MainSharedScene;

        [BoxGroup("Scene Management")]
        [ShowIf("LoadMode", SceneLoadMode.Separated)]
        public SubScene SharedSubScene; 
        
        [BoxGroup("Scene Management")]
        [ShowIf("LoadMode", SceneLoadMode.Separated)]
        public SubScene ServerSubScene; 
        
        [BoxGroup("Scene Management")]
        [ShowIf("LoadMode", SceneLoadMode.Separated)]
        public SubScene ClientSubScene; 
        #endregion
        #region Server Security
        [BoxGroup("Server Security", centerLabel: true)]
        [ShowIf("@CurrentRole == Role.Server || CurrentRole == Role.ServerClient")]
        [Tooltip("Если true - пускает всех, если пароль совпал. Если false - вызывает C# ивент OnPlayerRequestJoin для ручного контроля.")]
        public bool AutoAcceptConnections = true;

        [BoxGroup("Server Security")]
        [ShowIf("@CurrentRole == Role.Server || CurrentRole == Role.ServerClient")]
        [Tooltip("Оставьте пустым, чтобы пускать без пароля")]
        public string ServerPassword = "";

        [BoxGroup("Server Security")]
        [ShowIf("@CurrentRole == Role.Server || CurrentRole == Role.ServerClient")]
        public int MaxPlayers = 64;
        #endregion

        #region Client Profile (Default)
        [BoxGroup("Client Profile", centerLabel: true)]
        [ShowIf("@CurrentRole == Role.Client || CurrentRole == Role.ServerClient")]
        [InfoBox("Эти данные используются, если клиент стартует автоматически. Для UI используйте класс ConnectionBuilder.")]
        public string DefaultNickname; // Небольшой хак для тестов

        [BoxGroup("Client Profile")]
        [ShowIf("@CurrentRole == Role.Client || CurrentRole == Role.ServerClient")]
        public string DefaultPassword = "";
        #endregion

        // Кэшируем текущие данные для RPC (Этап 2)
        [HideInInspector] public string ActiveClientNickname;
        [HideInInspector] public string ActiveClientPassword;
        
        [HideInInspector] public World ServerWorld { get; private set; }
        [HideInInspector] public World ClientWorld { get; private set; }

        private void Awake()
        {
            DefaultNickname = "Player_" + Random.Range(1000, 9999);
            SetupSingleton();
            ApplyHeadlessDetection();
            ApplyFramerateSettings();
        }

        private void Start()
        {
            if (IsAutoStart) InitializeNetwork();
        }

        private void SetupSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // --- НОВЫЙ МЕТОД: Определение Headless ---
        private void ApplyHeadlessDetection()
        {
            if (AutoDetectHeadless && (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null || Application.isBatchMode))
            {
                CurrentRole = Role.Server;
                Debug.Log("[DotsBridge] Обнаружен Headless режим (без графики). Принудительный запуск в качестве выделенного сервера.");
            }
        }

        // --- ОБНОВЛЕННЫЙ МЕТОД: Лимит FPS ---
        private void ApplyFramerateSettings()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = LimitFPS ? TargetFPS : -1;
            Application.runInBackground = RunInBackground;
        }

        // --- НОВЫЙ МЕТОД: Получение LAN IP для инспектора ---
        // Использует легкий UDP-сокет, чтобы ОС сама вернула IP-адрес активного сетевого интерфейса (Wi-Fi/Ethernet)
        public string GetLocalIPAddress()
        {
            try
            {
                using (var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530); // Подключаемся к внешнему IP, чтобы маршрутизатор выдал локальный IP
                    var endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
                    return endPoint?.Address.ToString() ?? "127.0.0.1";
                }
            }
            catch
            {
                return "Не удалось определить (офлайн?)";
            }
        }

        [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
        [BoxGroup("General Configuration")]
        public void InitializeNetwork()
        {
            if (ClearWorldsOnStart) ClearOldWorlds();

            Debug.Log($"[DotsBridge] Инициализация сети в режиме: {CurrentRole}");

            switch (CurrentRole)
            {
                case Role.Server: StartServer(); break;
                case Role.Client: StartClient(); break;
                case Role.ServerClient: StartHost(); break;
            }
        }

        private void ClearOldWorlds()
        {
            var worldsToDispose = new List<World>();
            foreach (var world in World.All)
            {
                if (world.IsClient() || world.IsServer()) worldsToDispose.Add(world);
            }
            foreach (var world in worldsToDispose) world.Dispose(); 
        }

        private void ApplyTickRateToWorld(World world)
        {
            if (world == null || !world.IsCreated) return;

            var em = world.EntityManager;
            var tickRateSettings = new ClientServerTickRate
            {
                SimulationTickRate = this.SimulationTickRate,
                NetworkTickRate = this.NetworkTickRate,
                MaxSimulationStepsPerFrame = (byte)this.MaxBatchedTicks
            };

            // 1. Находим все сущности, у которых есть ClientServerTickRate
            using var query = em.CreateEntityQuery(typeof(ClientServerTickRate));
            
            // 2. Если они уже существуют (Netcode создал дефолтный или загрузилась сцена) — жестко удаляем их все, чтобы избежать дубликатов.
            if (!query.IsEmptyIgnoreFilter)
            {
                em.DestroyEntity(query);
            }

            // 3. Создаем ровно один, наш чистый экземпляр с нужными настройками
            var entity = em.CreateEntity(typeof(ClientServerTickRate));
            em.SetComponentData(entity, tickRateSettings);
            
            Debug.Log($"[DotsBridge] TickRate применен к миру {world.Name}. Старые дубликаты очищены.");
        }

        private void LoadSceneIntoWorld(World world, SubScene subScene)
        {
            if (world == null || !world.IsCreated || subScene == null) return;
            SceneSystem.LoadSceneAsync(world.Unmanaged, subScene.SceneGUID);
            Debug.Log($"[DotsBridge] Сцена {subScene.gameObject.name} загружается в мир {world.Name}");
        }

        private void HandleSceneLoading(World world, bool isServer)
        {
            if (LoadMode == SceneLoadMode.None) return;

            if (LoadMode == SceneLoadMode.SingleShared)
            {
                LoadSceneIntoWorld(world, MainSharedScene);
            }
            else if (LoadMode == SceneLoadMode.Separated)
            {
                LoadSceneIntoWorld(world, SharedSubScene);
                LoadSceneIntoWorld(world, isServer ? ServerSubScene : ClientSubScene);
            }
        }

        // --- ОБНОВЛЕННЫЕ МЕТОДЫ СЕТИ (С учетом UseLocalhost) ---
        private void StartServer()
        {
            ServerWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            ApplyTickRateToWorld(ServerWorld);
            HandleSceneLoading(ServerWorld, true); 
            
            var ep = NetworkEndpoint.AnyIpv4.WithPort(ServerPort); // Сервер ВСЕГДА слушает все интерфейсы
            var em = ServerWorld.EntityManager;
            var listenEntity = em.CreateEntity(typeof(NetworkStreamRequestListen));
            em.SetComponentData(listenEntity, new NetworkStreamRequestListen { Endpoint = ep });

            World.DefaultGameObjectInjectionWorld = ServerWorld;
        }

        private void StartClient()
        {
            ActiveClientNickname = DefaultNickname;
            ActiveClientPassword = DefaultPassword;
            
            string ipToConnect = UseLocalhost ? "127.0.0.1" : TargetServerIP;
            ExecuteClientConnection(ipToConnect, ServerPort);
        }

        private void StartHost()
        {
            ActiveClientNickname = "Host_" + DefaultNickname;
            ActiveClientPassword = ServerPassword;
            
            ServerWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            ClientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            
            ApplyTickRateToWorld(ServerWorld);
            ApplyTickRateToWorld(ClientWorld);

            HandleSceneLoading(ServerWorld, true);
            HandleSceneLoading(ClientWorld, false);

            var serverEp = NetworkEndpoint.AnyIpv4.WithPort(ServerPort);
            
            // Для хоста локальный клиент всегда коннектится по Localhost, так как сервер на этой же машине
            var clientEp = NetworkEndpoint.Parse("127.0.0.1", ServerPort); 

            var serverEm = ServerWorld.EntityManager;
            var listenEntity = serverEm.CreateEntity(typeof(NetworkStreamRequestListen));
            serverEm.SetComponentData(listenEntity, new NetworkStreamRequestListen { Endpoint = serverEp });

            var clientEm = ClientWorld.EntityManager;
            var connectEntity = clientEm.CreateEntity(typeof(NetworkStreamRequestConnect));
            clientEm.SetComponentData(connectEntity, new NetworkStreamRequestConnect { Endpoint = clientEp });

            World.DefaultGameObjectInjectionWorld = ClientWorld;
        }

 
        // Этот метод вызывается из твоего UI через ConnectionBuilder.Connect()
        public void StartClientFromBuilder(ConnectionBuilder builder)
        {
            if (ClearWorldsOnStart) ClearOldWorlds();

            CurrentRole = Role.Client;
            ActiveClientNickname = builder.Nickname;
            ActiveClientPassword = builder.Password;

            ExecuteClientConnection(builder.IP, builder.Port);
        }

        // Физическое создание клиентского мира и отправка NetworkStreamRequestConnect
        private void ExecuteClientConnection(string ip, ushort port)
        {
            ClientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            ApplyTickRateToWorld(ClientWorld);
            HandleSceneLoading(ClientWorld, false);

            var ep = NetworkEndpoint.Parse(ip, port);
            var em = ClientWorld.EntityManager;
            var connectEntity = em.CreateEntity(typeof(NetworkStreamRequestConnect));
            em.SetComponentData(connectEntity, new NetworkStreamRequestConnect { Endpoint = ep });

            World.DefaultGameObjectInjectionWorld = ClientWorld;
            Debug.Log($"[DotsBridge] Запрос подключения к {ip}:{port} как {ActiveClientNickname}");
        }
    }

    public static partial class EntityBridge
    {
        private static BridgeWorld[] _worldStates = new BridgeWorld[32];

        public static event Action<BridgeWorld> OnWorldCreated;
        public static event Action<BridgeWorld> OnWorldDestroyed;
        public static event Action OnServerStarted;
        public static event Action OnClientStarted;

        public static BridgeWorld InCurrentWorld()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return null;

            ulong seq = world.SequenceNumber;
            
            if (seq < (ulong)_worldStates.Length)
            {
                var bridge = _worldStates[seq];
                if (bridge != null) return bridge;
            }

            return GetOrCreateBridge(world);
        }

        internal static BridgeWorld GetOrCreateBridge(World targetWorld)
        {
            if (targetWorld == null || !targetWorld.IsCreated) return null;

            ulong seq = targetWorld.SequenceNumber;

            if (seq >= (ulong)_worldStates.Length)
            {
                Array.Resize(ref _worldStates, Mathf.NextPowerOfTwo((int)seq + 1));
            }

            if (_worldStates[seq] == null)
            {
                var bridge = new BridgeWorld(targetWorld);
                _worldStates[seq] = bridge;

                targetWorld.GetOrCreateSystemManaged<BridgeCleanupSystem>();

                OnWorldCreated?.Invoke(bridge);

                if ((targetWorld.Flags & WorldFlags.GameServer) != 0)
                {
                    OnServerStarted?.Invoke();
                }
                else if ((targetWorld.Flags & WorldFlags.GameClient) != 0)
                {
                    OnClientStarted?.Invoke();
                }
            }

            return _worldStates[seq];
        }

        public static void HandleWorldDestroyed(World world)
        {
            ulong seq = world.SequenceNumber;
            if (seq < (ulong)_worldStates.Length && _worldStates[seq] != null)
            {
                var bridge = _worldStates[seq];
                OnWorldDestroyed?.Invoke(bridge);
                
                bridge.Dispose();
                _worldStates[seq] = null;
                
                Debug.Log($"[DotsBridge] Мир {world.Name} (Seq: {seq}) уничтожен. Мост очищен.");
            }
        }

        public static void DisposeAllStates()
        {
            for (int i = 0; i < _worldStates.Length; i++)
            {
                if (_worldStates[i] != null)
                {
                    _worldStates[i].Dispose();
                    _worldStates[i] = null;
                }
            }
        }

        public static BridgeWorld InServerWorld()
        {
            var serverWorld = DotsBridgeBootstrapper.Instance?.ServerWorld;
            return serverWorld != null ? GetOrCreateBridge(serverWorld) : null;
        }

        public static BridgeWorld InClientWorld()
        {
            var clientWorld = DotsBridgeBootstrapper.Instance?.ClientWorld;
            return clientWorld != null ? GetOrCreateBridge(clientWorld) : null;
        }
    }
}
