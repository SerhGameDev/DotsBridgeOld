using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DotsBridge.Modules.Network
{
    [DefaultExecutionOrder(-90)] 
    public class DotsNetworkWrapper : MonoBehaviour
    {
        public static DotsNetworkWrapper Instance { get; private set; }

        [Header("Singleton Settings")]
        public bool IsSingleton = true;

        [Header("Network Mode")]
        [SerializeField] private Role _role = Role.ServerClient;
        public bool IsAutoStart = true;

        [Header("Connection Settings")]
        public string ServerIP = "127.0.0.1";
        public ushort ServerPort = 7979;

        [Header("Tick Rate Settings (Applied to Netcode)")]
        public int SimulationTickRate = 60;
        public int NetworkTickRate = 60;
        public int MaxBatchedTicks = 5;

        private void Awake()
        {
            QualitySettings.vSyncCount = 0; 
            Application.targetFrameRate = 120;
            Application.runInBackground = true;
            SetupSingleton();
        }

        private void Start()
        {
            if (IsAutoStart)
            {
                StartNetwork();
            }
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
            DontDestroyOnLoad(gameObject);
        }
        public void ApplyTickRate()
        {
            var serverWorld = EntityBridge.InServerWorld().World; 
    
            if (serverWorld != null && serverWorld.IsCreated)
            {
                var em = serverWorld.EntityManager;
        
                var tickRateSettings = new ClientServerTickRate
                {
                    SimulationTickRate = this.SimulationTickRate,
                    NetworkTickRate = this.NetworkTickRate,
                    // Вот правильное название поля:
                    MaxSimulationStepsPerFrame = (byte)this.MaxBatchedTicks 
                };

                var query = em.CreateEntityQuery(typeof(ClientServerTickRate));
        
                if (query.HasSingleton<ClientServerTickRate>())
                {
                    query.SetSingleton(tickRateSettings);
                }
                else
                {
                    var entity = em.CreateEntity(typeof(ClientServerTickRate));
                    em.SetComponentData(entity, tickRateSettings);
                }
            }
        }
        /// <summary>
        /// Публичный метод. Удобно вызывать с кнопок UI.
        /// </summary>
        public void StartNetwork()
        {
            switch (_role)
            {
                case Role.Server:
                    DotsNetworkManager.StartServer(ServerPort);
                    break;

                case Role.Client:
                    DotsNetworkManager.ConnectClient(ServerIP, ServerPort);
                    break;

                case Role.ServerClient:
                    DotsNetworkManager.StartHost(ServerIP, ServerPort);
                    break;
            }

            ApplyTickRate();
        }
    }
}