using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Placement
{
    /// <summary>
    /// Размер объекта для проверки коллизий при размещении (AABB).
    /// </summary>
    public struct PlacementBounds : IComponentData
    {
        public float3 Size;
    }

    // --- BAKER ДЛЯ ГРАНИЦ ---
}