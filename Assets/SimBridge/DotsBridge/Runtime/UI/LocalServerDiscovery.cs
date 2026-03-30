using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using DotsBridge; // Для проверки статуса сервера

public class LocalServerDiscovery : MonoBehaviour
{
    public static LocalServerDiscovery Instance { get; private set; }

    [Header("Настройки")]
    public int BroadcastPort = 47777;
    public float BroadcastInterval = 1.0f; // Раз в секунду
    public float ServerTimeout = 3.0f;     // Если нет сигнала 3 сек - удаляем из списка

    // Структура для хранения информации о найденном сервере
    public class DiscoveredServer
    {
        public string IP;
        public string Name;
        public ushort Port;
        public float LastSeenTime;
    }

    // Словарь активных серверов в сети (IP -> Данные)
    public Dictionary<string, DiscoveredServer> ActiveServers { get; private set; } = new Dictionary<string, DiscoveredServer>();
    
    // Событие для обновления UI
    public event Action OnServersUpdated;

    private UdpClient _udpClient;
    private float _broadcastTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        try
        {
            // Открываем UDP порт для прослушивания (разрешаем переиспользование, если запущено 2 окна игры на одном ПК)
            _udpClient = new UdpClient();
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, BroadcastPort));
        }
        catch (Exception e)
        {
            Debug.LogError($"[Discovery] Не удалось открыть порт {BroadcastPort}: {e.Message}");
        }
    }

    private void OnDisable()
    {
        if (_udpClient != null)
        {
            _udpClient.Close();
            _udpClient = null;
        }
    }

    private void Update()
    {
        CleanupStaleServers();

        // РЕЖИМ СЕРВЕРА: Рассылаем маяк
        if (ServerBridge.IsActive)
        {
            _broadcastTimer += Time.deltaTime;
            if (_broadcastTimer >= BroadcastInterval)
            {
                _broadcastTimer = 0f;
                BroadcastPresence();
            }
        }
        // РЕЖИМ КЛИЕНТА (В МЕНЮ): Слушаем эфир
        else if (!ClientBridge.IsActive)
        {
            ListenForBroadcasts();
        }
    }

    private void BroadcastPresence()
    {
        if (_udpClient == null) return;

        // Формируем пакет данных: "ИмяСервера|Порт"
        // Позже сюда можно добавить онлайн: "Имя|Порт|Игроков:2/64"
        string serverName = PlayerPrefs.GetString("PlayerNickname", "Server") + "'s Room";
        ushort hostPort = DotsBridge.Core.MonoBehaviourBridge.Instance.Network.ServerPort;
        
        string message = $"SIMOIL_BEACON|{serverName}|{hostPort}";
        byte[] data = Encoding.UTF8.GetBytes(message);

        try
        {
            // Отправляем пакет ВСЕМ в локальной сети (Broadcast)
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, BroadcastPort);
            _udpClient.Send(data, data.Length, endPoint);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Discovery] Ошибка отправки маяка: {e.Message}");
        }
    }

    private void ListenForBroadcasts()
    {
        if (_udpClient == null) return;

        // Читаем все пакеты, которые успели прийти в буфер
        while (_udpClient.Available > 0)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = _udpClient.Receive(ref remoteEndPoint);
            string message = Encoding.UTF8.GetString(data);

            // Проверяем, что это наш пакет, а не чужой мусор в сети
            if (message.StartsWith("SIMOIL_BEACON"))
            {
                string[] parts = message.Split('|');
                if (parts.Length >= 3)
                {
                    string ip = remoteEndPoint.Address.ToString();
                    
                    if (!ActiveServers.ContainsKey(ip))
                    {
                        ActiveServers[ip] = new DiscoveredServer();
                    }

                    // Обновляем данные
                    ActiveServers[ip].IP = ip;
                    ActiveServers[ip].Name = parts[1];
                    ushort.TryParse(parts[2], out ActiveServers[ip].Port);
                    ActiveServers[ip].LastSeenTime = Time.realtimeSinceStartup;

                    OnServersUpdated?.Invoke(); // Сообщаем UI, что список изменился
                }
            }
        }
    }

    private void CleanupStaleServers()
    {
        bool changed = false;
        List<string> toRemove = new List<string>();

        foreach (var kvp in ActiveServers)
        {
            if (Time.realtimeSinceStartup - kvp.Value.LastSeenTime > ServerTimeout)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var ip in toRemove)
        {
            ActiveServers.Remove(ip);
            changed = true;
        }

        if (changed) OnServersUpdated?.Invoke();
    }
}