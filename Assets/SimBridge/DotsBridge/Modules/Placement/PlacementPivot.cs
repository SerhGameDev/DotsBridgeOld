using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Placement
{
    /// <summary>
    /// Задает локальное смещение меша относительно точки прикосновения луча.
    /// </summary>
    public struct PlacementPivot : IComponentData
    {
        public float3 Value;
    }

    // Скрипт, который нужно повесить на ПРЕФАБ реле/короба, чтобы настроить смещение в Инспекторе
}