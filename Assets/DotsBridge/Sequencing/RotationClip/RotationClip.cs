using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Timeline
{
    // Буфер для вращения
    [InternalBufferCapacity(8)]
    public struct RotationClip : IBufferElementData
    {
        public float StartTime;
        public float Duration;

        // Храним в градусах (Euler), чтобы можно было крутить > 360
        public float3 StartEuler;
        public float3 EndEuler;

        public Ease Easing;
        public float EndTime => StartTime + Duration;
    }

    // Состояние вращения (Отдельный компонент, чтобы крутиться независимо от ходьбы)
    public struct RotationTimelineState : IComponentData, IEnableableComponent
    {
        public float CurrentTime;
        public float PlaybackSpeed;
        public int CurrentClipIndex;
        public int Loops; // -1 = Infinite
        public bool IsPlaying;
    }

    // Начальное вращение (Для относительного поворота)
    public struct RotationOrigin : IComponentData
    {
        public quaternion Value;
        public bool IsCaptured;
    }
}