using UnityEngine;

namespace ModbusBridge
{
    public class ModbusBridgeRunner : MonoBehaviour
    {
        [Tooltip("Останавливать все соединения при выходе или уничтожении объекта?")]
        public bool deactivateOnDestroy = true;

        private void Awake()
        {
            // Делаем драйвер бессмертным при переходе между сценами (опционально)
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // Каждый кадр проталкиваем события из фоновой сети в главный поток
            ModbusService.UpdateMainThreadQueue();
        }

        private void OnDestroy()
        {
            // Безопасно тушим сеть при закрытии игры/сцены
            if (deactivateOnDestroy)
            {
                ModbusService.ClearAll();
            }
        }
    }
}