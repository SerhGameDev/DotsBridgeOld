using System;
using System.Collections.Generic;

namespace DotsBridge.Modules.Network
{
    public static class ConnectionManager
    {
        // Список всех подключенных игроков (ключ - NetworkId)
        public static readonly Dictionary<int, PlayerSession> Players = new Dictionary<int, PlayerSession>();

        // ================= СЕРВЕРНЫЕ СОБЫТИЯ =================
        
        // Срабатывает, когда игрок успешно прошел авторизацию и добавлен в игру
        public static event Action<PlayerSession> OnPlayerJoined;
        
        // Срабатывает, когда игрок отключился (по своей воле или по тайм-ауту)
        public static event Action<int, string> OnPlayerLeft; // NetworkId, Причина
        
        // Срабатывает, если AutoAcceptConnections = false. Требует вызова Accept или Reject.
        public static event Action<AuthRequestData> OnPlayerRequestJoin; 

        // ================= КЛИЕНТСКИЕ СОБЫТИЯ =================
        
        // Срабатывает, когда мы (как клиент) успешно зашли на сервер
        public static event Action OnConnectedToServer;
        
        // Срабатывает, если нас отключили или отказали во входе
        public static event Action<string> OnDisconnectedFromServer; // Причина (WrongPassword, Full и т.д.)

        public static void Clear()
        {
            Players.Clear();
        }
    }
}