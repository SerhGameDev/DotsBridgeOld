using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Placement
{
    /// <summary>
    /// Тег для объекта, на который МОЖНО ставить другие объекты (стена, пол, монтажная панель, дверь).
    /// </summary>
    public struct SurfaceTag : IComponentData { }

    /// <summary>
    /// Тег для объекта, который можно размещать в пространстве.
    /// </summary>
    public struct PlaceableTag : IComponentData { }

    /// <summary>
    /// Маркер "фантома". Показывает, что этот объект сейчас привязан к курсору 
    /// и еще не закреплен окончательно (не является частью симуляции).
    /// </summary>
    public struct GhostTag : IComponentData 
    { 
        public bool IsValid; 
    }

    /// <summary>
    /// Настройки привязки к сетке (Snapping).
    /// </summary>
    public struct GridSnapSettings : IComponentData
    {
        public float Step;
        public bool IsEnabled;
    }

    /// <summary>
    /// Состояние для открывающихся элементов (дверцы шкафа, крышки).
    /// </summary>
    public struct DoorState : IComponentData
    {
        public float CurrentAngle;
        public float TargetAngle;
        public float Speed;
        // Ось вращения (обычно math.up() для стандартных дверей)
        public float3 Axis; 
    }

    /// <summary>
    /// Данные для растягиваемых объектов (короба для проводов, кабели).
    /// </summary>
    public struct StretchableDuct : IComponentData
    {
        public float3 StartPoint;
        public float3 EndPoint;
        public float Width;
        public float Height;
        public bool IsDrawing; 
    }
}