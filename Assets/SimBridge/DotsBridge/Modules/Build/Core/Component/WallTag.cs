using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Build
{
    // Тег для фильтрации
    public struct WallTag : IComponentData { }

    // Основные данные стены
    public struct WallComponent : IComponentData
    {
        public float3 Start;
        public float3 End;
        public float Thickness;
    }

    // Элемент буфера для хранения связей (Граф)
    [InternalBufferCapacity(4)] // Обычно у угла стены не более 4-х соединений
    public struct ConnectionElement : IBufferElementData
    {
        public Entity ConnectedWall;
    }
}