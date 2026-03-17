using Unity.Collections;
using Unity.NetCode;

namespace DotsBridge.Tests
{
    public struct TestMessageRpc : IRpcCommand
    {
        public int Value;
    }/// <summary>
     /// Клиент отправляет серверу свой никнейм при подключении.
     /// </summary>
    public struct SendNameRpc : IRpcCommand
    {
        public FixedString32Bytes Name;
    }

    /// <summary>
    /// Сервер рассылает клиентам информацию об изменении списка игроков.
    /// </summary>
    public struct SyncPlayerRpc : IRpcCommand
    {
        public int NetworkId;
        public FixedString32Bytes Name;
        public bool IsJoining; // true = добавился, false = вышел
    }
}