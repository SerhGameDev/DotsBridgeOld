using Unity.Entities;

namespace DotsBridge.Build
{
    // Тег для фильтрации и поиска комнат
    public struct RoomTag : IComponentData { }

    // Основные физические данные комнаты
    public struct RoomData : IComponentData
    {
        public float Area;
        public float Perimeter;
    }

    // Буфер, хранящий ссылки на стены, образующие контур комнаты
    [InternalBufferCapacity(8)]
    public struct RoomWallElement : IBufferElementData
    {
        public Entity WallEntity;
    }
    
    // Тег-маркер, чтобы система понимала, что граф стен изменился и нужен пересчет
    public struct TopologyChangedTag : IComponentData { }
}