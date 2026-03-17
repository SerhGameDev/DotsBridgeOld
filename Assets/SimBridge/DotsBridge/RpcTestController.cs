using DotsBridge;
using DotsBridge.Tests;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using System;

namespace DotsBridge
{
    public class RpcTestController : MonoBehaviour
    {
        private Action<TestMessageRpc, SingleEntity> _onRpcReceived;
        private bool _isServerSubscribed = false;

        private void Awake()
        {
            _onRpcReceived = OnMessageReceived;
        }

        private void OnEnable()
        {
            EntityBridge.OnServerStarted += OnServerStarted;
            EntityBridge.OnClientStarted += OnClientStarted;

            if (EntityBridge.InServerWorld() != null) OnServerStarted();
            if (EntityBridge.InClientWorld() != null) OnClientStarted();
        }

        private void OnServerStarted()
        {
            if (_isServerSubscribed) return;
            _isServerSubscribed = true;

            Debug.Log("[Test] Сервер запущен. Подписываюсь на TestMessageRpc...");
            EntityBridge.InServerWorld().SubscribeRpc<TestMessageRpc>(_onRpcReceived);
        }

        private void OnMessageReceived(TestMessageRpc message, SingleEntity sender)
        {
            Debug.Log($"<color=green>[Server]</color> Получено число {message.Value} от Клиента ID: {sender.GetNetworkId()}");
        }

        private void OnClientStarted()
        {
            Debug.Log("[Test] Клиент запущен. Подключение к серверу...");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var clientBridge = EntityBridge.InClientWorld();

                // Проверяем, установлено ли уже соединение с сервером
                var targetConn = EntityBridge.GetServerConnection(clientBridge);
                if (targetConn == Entity.Null)
                {
                    Debug.LogWarning("<color=yellow>[Test]</color> Соединение с сервером еще не установлено! Подождите пару секунд и нажмите снова.");
                    return;
                }

                int randomVal = UnityEngine.Random.Range(1, 100);
                Debug.Log($"[Test] Отправляю RPC с числом {randomVal}");
                clientBridge.SendRpc(new TestMessageRpc { Value = randomVal });
            }
        }

        private void OnDisable()
        {
            EntityBridge.OnServerStarted -= OnServerStarted;
            EntityBridge.OnClientStarted -= OnClientStarted;

            EntityBridge.InServerWorld()?.UnsubscribeRpc<TestMessageRpc>(_onRpcReceived);
            _isServerSubscribed = false;
        }
    }
}