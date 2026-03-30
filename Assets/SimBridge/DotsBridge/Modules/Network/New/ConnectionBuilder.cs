using DotsBridge.Core;

namespace DotsBridge.Modules.Network
{
    public class ConnectionBuilder
    {
        public string IP { get; private set; } = "127.0.0.1";
        public ushort Port { get; private set; } = 7979;
        public string Nickname { get; private set; } = "Player";
        public string Password { get; private set; } = "";

        public ConnectionBuilder SetIP(string ip) { IP = ip; return this; }
        public ConnectionBuilder SetPort(ushort port) { Port = port; return this; }
        public ConnectionBuilder SetNickname(string nickname) { Nickname = nickname; return this; }
        public ConnectionBuilder SetPassword(string password) { Password = password; return this; }

        // Метод для финального запуска подключения
        public void Connect()
        {
            MonoBehaviourBridge.Instance.StartClientFromUI(IP, Port, Nickname, Password);
        }
    }
}