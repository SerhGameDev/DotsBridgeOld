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
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 1;
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
        }
    }
}