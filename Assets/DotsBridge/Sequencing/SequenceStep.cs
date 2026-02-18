using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Timeline
{
    // Типы плавности
    public enum Ease : byte
    {
        Linear,
        InQuad, OutQuad, InOutQuad,
        InSine, OutSine, InOutSine,
        InExp, OutExp, InOutExp
    }

    // 1. ДИРИЖЕР (Состояние таймлайна)
    public struct TimelineState : IComponentData, IEnableableComponent
    {
        public float CurrentTime;
        public float PlaybackSpeed;
        public int CurrentClipIndex;
        public bool IsPlaying;
        public int Loops; 
    }
    // 2. ТОЧКА ОТСЧЕТА (Lazy Origin)
    // Система сама заполнит это поле в первом кадре анимации
    public struct SequenceOrigin : IComponentData
    {
        public float3 Value;
        public bool IsCaptured;
    }

    // 3. КЛИП ДВИЖЕНИЯ (Данные в буфере)
    [InternalBufferCapacity(16)] // 16 шагов хранятся супер-быстро в чанке
    public struct RelativeMoveClip : IBufferElementData
    {
        public float StartTime;
        public float Duration;

        // Смещения относительно Origin
        public float3 StartOffset;
        public float3 EndOffset;

        public Ease Easing;

        // Хелпер
        public float EndTime => StartTime + Duration;
    }
}