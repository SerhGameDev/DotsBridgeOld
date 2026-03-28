using Unity.Entities;

namespace DotsBridge.Modules.Network
{
    // Данные сессии конкретного игрока
    public class PlayerSession
    {
        public int NetworkId;          // Уникальный ID соединения (от Netcode)
        public string Nickname;        // Никнейм игрока
        
        // Transient Entity: Сущность самого сетевого соединения. 
        // Если игрок отключается, Netcode её уничтожает.
        public Entity ConnectionEntity; 
        
        // Persistent Entity: Сущность аватара/профиля/инвентаря.
        // Мы можем решить не удалять её при дисконнекте, чтобы игрок мог "вернуться" в тело.
        public Entity ProfileEntity;    
    }

    // Временные данные запроса (для ручного одобрения входа)
}
