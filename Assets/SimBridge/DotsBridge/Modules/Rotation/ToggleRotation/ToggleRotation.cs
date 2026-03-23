using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge
{
    public struct ToggleRotation : IComponentData
    {
        public bool IsOn; // Наш bool сигнал
        public float Speed; // Скорость вращения (в радианах в секунду)
        public float3 Axis; // Ось вращения (например, math.up() или math.forward())
    }
}

namespace DotsBridge
    {
    }