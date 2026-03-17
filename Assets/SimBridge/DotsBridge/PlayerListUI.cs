//using DotsBridge.Tests;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using TMPro;
//using Unity.Collections;
//using UnityEngine;

//namespace DotsBridge
//{
//    public class PlayerListUI : MonoBehaviour
//    {
//        [Header("UI References")]
//        public TextMeshProUGUI PlayerListText;
//        public string MyNickname = "Player_" + UnityEngine.Random.Range(100, 999);

//        private Dictionary<int, string> _players = new Dictionary<int, string>();

//        private Action<SendNameRpc, SingleEntity> _onNameReceived;
//        private Action<SyncPlayerRpc, SingleEntity> _onSyncPlayerReceived;
//        private Action<SingleEntity, int> _onClientConnected;
//        private Action<int> _onClientDisconnected;

//        private void Awake()
//        {
//            _onNameReceived = OnReceiveName;
//            _onSyncPlayerReceived = OnSyncPlayer;
//        }

//        private void OnEnable()
//        {
//            EntityBridge.OnServerStarted += SetupServer;
//            EntityBridge.OnClientStarted += SetupClient;

//            EntityBridge.OnClientConnected += _onClientConnected = HandleClientConnected;
//            EntityBridge.OnClientDisconnected += _onClientDisconnected = HandleClientDisconnected;
//        }

//        private void SetupServer()
//        {
//            EntityBridge.InServerWorld().SubscribeRpc<SendNameRpc>(_onNameReceived);
//        }

//        private void HandleClientConnected(SingleEntity connection, int networkId)
//        {
//            // Игрок только что подключился. Выдаем временное имя.
//            _players[networkId] = $"Player {networkId}";
//            Debug.Log($"<color=green>[Server]</color> Системное подключение: ID {networkId}");

//            // Сообщаем всем клиентам о новичке
//            EntityBridge.InServerWorld()?.BroadcastRpc(new SyncPlayerRpc
//            {
//                NetworkId = networkId,
//                Name = new FixedString32Bytes(_players[networkId]),
//                IsJoining = true
//            });

//            UpdateUI();
//        }

//        private void HandleClientDisconnected(int networkId)
//        {
//            if (_players.ContainsKey(networkId))
//            {
//                _players.Remove(networkId);
//                Debug.Log($"<color=orange>[Server]</color> Системное отключение: ID {networkId}");

//                // Говорим всем клиентам удалить этого игрока из UI
//                EntityBridge.InServerWorld()?.BroadcastRpc(new SyncPlayerRpc
//                {
//                    NetworkId = networkId,
//                    IsJoining = false
//                });

//                UpdateUI();
//            }
//        }

//        private void OnReceiveName(SendNameRpc rpc, SingleEntity sender)
//        {
//            int netId = sender.GetNetworkId();
//            string newName = rpc.Name.ToString();

//            // Обновляем временное имя на настоящее
//            _players[netId] = newName;
//            Debug.Log($"<color=green>[Server]</color> Игрок {netId} установил никнейм: {newName}");

//            var serverBridge = EntityBridge.InServerWorld();

//            // 1. Отправляем НОВОМУ игроку список всех, кто УЖЕ на сервере
//            foreach (var p in _players)
//            {
//                if (p.Key != netId)
//                {
//                    serverBridge.SendRpcToClient(new SyncPlayerRpc
//                    {
//                        NetworkId = p.Key,
//                        Name = new FixedString32Bytes(p.Value),
//                        IsJoining = true
//                    }, netId); // Отправляем лично ему
//                }
//            }

//            // 2. Рассылаем всем остальным обновленный никнейм новичка
//            serverBridge.BroadcastRpc(new SyncPlayerRpc
//            {
//                NetworkId = netId,
//                Name = rpc.Name,
//                IsJoining = true
//            });

//            UpdateUI();
//        }


//        private void SetupClient()
//        {
//            EntityBridge.InClientWorld().SubscribeRpc(_onSyncPlayerReceived);

//            EntityBridge.InClientWorld().SendRpc(new SendNameRpc { Name = new FixedString32Bytes(MyNickname) });
//        }

//        private void OnSyncPlayer(SyncPlayerRpc rpc, SingleEntity sender)
//        {
//            if (rpc.IsJoining)
//            {
//                _players[rpc.NetworkId] = rpc.Name.ToString();
//            }
//            else
//            {
//                _players.Remove(rpc.NetworkId);
//            }

//            UpdateUI();
//        }

//        // =========================================================
//        // ОТРИСОВКА И ОЧИСТКА
//        // =========================================================

//        private void UpdateUI()
//        {
//            if (PlayerListText == null) return;

//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine("<b>ИГРОКИ НА СЕРВЕРЕ:</b>");

//            foreach (var kvp in _players)
//            {
//                sb.AppendLine($"[{kvp.Key}] {kvp.Value}");
//            }

//            PlayerListText.text = sb.ToString();
//        }

//        private void OnDisable()
//        {
//            EntityBridge.OnServerStarted -= SetupServer;
//            EntityBridge.OnClientStarted -= SetupClient;

//            EntityBridge.OnClientConnected -= _onClientConnected;
//            EntityBridge.OnClientDisconnected -= _onClientDisconnected;

//            EntityBridge.InServerWorld()?.UnsubscribeRpc<SendNameRpc>(_onNameReceived);
//            EntityBridge.InClientWorld()?.UnsubscribeRpc<SyncPlayerRpc>(_onSyncPlayerReceived);
//        }
//    }
//}