using UnityEngine;
using DotsBridge;
using DotsBridge.Modules.Network;

public class NetworkTester : MonoBehaviour
{
    public DotsNetworkManager NetManager;

    private void OnEnable()
    {
        // Подписываемся на твой OOP-ивент моста
        DotsNetworkManager.OnClientConnected += OnConnectedSuccess;
    }

    private void OnDisable()
    {
        DotsNetworkManager.OnClientConnected -= OnConnectedSuccess;
    }

    void Update()
    {
        // Нажимаем 'S', чтобы запустить Сервер
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Запускаем сервер...");
            NetManager.StartServer();
        }

        // Нажимаем 'C', чтобы Клиент подключился к Серверу
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Клиент пытается подключиться...");
            NetManager.ConnectToServer();
        }
    }

    private void OnConnectedSuccess()
    {
        Debug.Log("<color=green>УРА! Клиент в игре!</color>");
    }
}