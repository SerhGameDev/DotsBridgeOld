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
        public int3 Value;

        public GridPosition(int3 position) => Value = position;

        public float3 ToWorldPosition(float cellSize)
        {
            return new float3(Value.x, Value.y, Value.z) * cellSize;
        }

        public static GridPosition FromWorldPosition(float3 worldPos, float cellSize)
        {
            return new GridPosition(new int3(
                (int)math.round(worldPos.x / cellSize),
                (int)math.round(worldPos.y / cellSize),
                (int)math.round(worldPos.z / cellSize)
            ));
        }
    }
    public struct SpatialGridData : IComponentData
    {
        public NativeParallelMultiHashMap<int3, Entity> Map;
    }
    public struct FloorTag : IComponentData { }
}