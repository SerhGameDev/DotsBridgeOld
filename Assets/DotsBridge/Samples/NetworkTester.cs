using UnityEngine;
using DotsBridge;
using DotsBridge.Modules.Network;

public class NetworkTester : MonoBehaviour
{

    private void OnEnable()
    {
        // Подписываемся на твой OOP-ивент моста
        DotsNetworkManager.OnClientConnected += OnConnectedSuccess;
    }

    private void OnDisable()
    {
        DotsNetworkManager.OnClientConnected -= OnConnectedSuccess;
    }


    private void OnConnectedSuccess()
    {
        Debug.Log("<color=green>УРА! Клиент в игре!</color>");
    }
}