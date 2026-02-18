using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Timeline
{
    public enum Ease : byte
    {
        Linear,
        InQuad, OutQuad, InOutQuad,
        InSine, OutSine, InOutSine,
        InExp, OutExp, InOutExp
    }

    public struct TimelineState : IComponentData, IEnableableComponent
    {
        public float CurrentTime;
        public float PlaybackSpeed;
        public int CurrentClipIndex;
        public bool IsPlaying;
        public int Loops; 
    }

    public struct SequenceOrigin : IComponentData
    {
        public float3 Value;
        public bool IsCaptured;
    }

    [InternalBufferCapacity(16)] 
    public struct RelativeMoveClip : IBufferElementData
    {
        public float StartTime;
        public float Duration;

        public float3 StartOffset;
        public float3 EndOffset;

        public Ease Easing;

        public float EndTime => StartTime + Duration;
    }
}