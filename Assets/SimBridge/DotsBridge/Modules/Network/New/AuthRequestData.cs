using Unity.Entities;

namespace DotsBridge.Modules.Network
{
    public struct AuthRequestData
    {
        public int NetworkId;
        public string Nickname;
        public string Password;
        public Entity ConnectionEntity;
    }
}