using UnityEngine;
using DotsBridge;
using DotsBridge.Core;

public class ServerWorldInitializer : MonoBehaviour
{
    private void OnEnable()
    {
        // Подписываемся на запуск сервера
        ServerBridge.OnStarted += InitializeOilWorld;
    }

    private void OnDisable()
    {
        ServerBridge.OnStarted -= InitializeOilWorld;
    }

    private void InitializeOilWorld()
    {
        Debug.Log("[Initializer] Сервер запущен, начинаю генерацию труб...");

        // Получаем ссылки на префабы из конфига нашего моста
        var scenes = MonoBehaviourBridge.Instance.Scenes;

    }
}