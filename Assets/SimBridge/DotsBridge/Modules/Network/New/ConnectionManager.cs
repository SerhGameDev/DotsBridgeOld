using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

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
        
        internal static void InvokeConnectedToServer() => OnConnectedToServer?.Invoke();
        internal static void InvokeDisconnectedFromServer(string reason) => OnDisconnectedFromServer?.Invoke(reason);
        internal static void InvokePlayerJoined(PlayerSession session) => OnPlayerJoined?.Invoke(session);
        internal static void InvokePlayerLeft(int id, string reason) => OnPlayerLeft?.Invoke(id, reason);
        internal static void InvokePlayerRequestJoin(AuthRequestData data) => OnPlayerRequestJoin?.Invoke(data);
        
        public static void Clear()
        {
            Players.Clear();
        }
    }
    public enum AuthStatus : byte
    {
        None,
        Success,
        WrongPassword,
        ServerFull,
        Rejected 
    }

    // Письмо от Клиента к Серверу: "Впусти меня, вот мои данные"
    public struct AuthRequestRpc : IRpcCommand
    {
        public FixedString64Bytes Nickname;
        public FixedString64Bytes Password;
    }

    // Письмо от Сервера к Клиенту: "Твой статус такой-то"
    public struct AuthResponseRpc : IRpcCommand
    {
        public AuthStatus Status;
        public FixedString64Bytes Reason; // Опциональный текст ошибки для UI
    }
    
    // Вспомогательный тег, чтобы мы не спамили запросами каждый кадр
    public struct AuthRequestSentTag : IComponentData { }
}