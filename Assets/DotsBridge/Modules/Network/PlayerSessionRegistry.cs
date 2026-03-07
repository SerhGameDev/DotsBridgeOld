using System.Collections.Generic;
using Unity.Entities;

namespace DotsBridge.Modules.Network
{
    public class PlayerSessionData
    {
        public int NetworkId;
        public string Nickname;
        public Entity AvatarEntity;
    }

    public static class PlayerSessionRegistry
    {
        private static readonly Dictionary<int, PlayerSessionData> _sessions = new();

        public static void Register(int networkId, string nickname = "Unknown")
        {
            if (!_sessions.ContainsKey(networkId))
            {
                _sessions[networkId] = new PlayerSessionData
                {
                    NetworkId = networkId,
                    Nickname = nickname
                };
            }
        }

        public static void Unregister(int networkId)
        {
            _sessions.Remove(networkId);
        }

        public static PlayerSessionData GetPlayer(int networkId)
        {
            return _sessions.TryGetValue(networkId, out var data) ? data : null;
        }

        public static void LinkAvatar(int networkId, Entity avatar)
        {
            if (_sessions.TryGetValue(networkId, out var data))
            {
                data.AvatarEntity = avatar;
            }
        }

        public static bool HasPlayer(int networkId) => _sessions.ContainsKey(networkId);
    }
}