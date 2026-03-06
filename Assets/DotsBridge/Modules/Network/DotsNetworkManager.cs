using UnityEngine;

namespace DotsBridge.Modules.Network
{
    public class DotsNetworkManager : MonoBehaviour
    {
        [Header("Singleton Settings")]
        [Tooltip("Сделать этот объект доступным из любого места кода")]
        public bool UseAsSingleton = true;
        [Tooltip("Не уничтожать объект при загрузке новых сцен")]
        public bool DontDestroy = true;

        // Глобальная ссылка на текущие настройки
        public static DotsNetworkManager Instance { get; private set; }

        [Header("Connection Settings")]
        public string ServerIP = "127.0.0.1";
        public ushort ServerPort = 7979;

        [Header("Tick Rate Settings")]
        [Tooltip("Частота обновления физики и логики в секунду")]
        public int SimulationTickRate = 60;

        [Tooltip("Как часто сервер отправляет пакеты клиентам")]
        public int NetworkTickRate = 60;

        private void Awake()
        {
            Application.runInBackground = true;

            // Логика инициализации Синглтона
            if (UseAsSingleton)
            {
                if (Instance != null && Instance != this)
                {
                    Debug.LogWarning("[DotsBridge] Обнаружен дубликат DotsNetworkManager! Удаляем...");
                    Destroy(gameObject);
                    return;
                }

                Instance = this;

                if (DontDestroy)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            // Очищаем ссылку, если объект уничтожен (чтобы не было утечек)
            if (UseAsSingleton && Instance == this)
            {
                Instance = null;
            }
        }

        // Вызывается из кнопок кастомного инспектора
        public void ConnectToServer()
        {
            NetEntityBridge.ConnectToServer(ServerIP, ServerPort, SimulationTickRate, NetworkTickRate);
        }

        public void StartServer()
        {
            NetEntityBridge.StartServer(ServerPort, SimulationTickRate, NetworkTickRate);
        }
    }
}