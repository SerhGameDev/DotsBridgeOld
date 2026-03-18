using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Build
{
    public struct GridSettings : IComponentData
    {
        public float CellSize;
    }

    public struct GridPosition : IComponentData
    {
        public float3 PrecisionPos; // Точная позиция для визуала и снаппинга
        public int3 GridIndex;      // Индекс в пространственной карте

        public GridPosition(float3 worldPos, float cellSize)
        {
            PrecisionPos = worldPos;
            GridIndex = new int3(
                (int)math.round(worldPos.x / cellSize),
                (int)math.round(worldPos.y / cellSize),
                (int)math.round(worldPos.z / cellSize)
            );
        }
    }
    public struct SpatialGridData : IComponentData
    {
        public NativeParallelMultiHashMap<int3, Entity> Map;
    }
    public struct FloorTag : IComponentData { }
}