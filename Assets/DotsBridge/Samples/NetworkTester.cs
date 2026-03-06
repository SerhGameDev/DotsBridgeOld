using UnityEngine;
using DotsBridge;
using DotsBridge.Modules.Network;

public class NetworkTester : MonoBehaviour
{

    private void OnEnable()
    {
        // Подписываемся на твой OOP-ивент моста
        NetEntityBridge.OnClientConnected += OnConnectedSuccess;
    }

    private void OnDisable()
    {
        NetEntityBridge.OnClientConnected -= OnConnectedSuccess;
    }

    void Update()
    {
        // Нажимаем 'S', чтобы запустить Сервер
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Запускаем сервер...");
            NetEntityBridge.StartServer();
        }

        // Нажимаем 'C', чтобы Клиент подключился к Серверу
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Клиент пытается подключиться...");
            NetEntityBridge.ConnectToServer();
        }
    }

    private void OnConnectedSuccess()
    {
        Debug.Log("<color=green>УРА! Клиент в игре!</color>");
    }
}