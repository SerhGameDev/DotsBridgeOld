using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Timeline
{
    [InternalBufferCapacity(8)]
    public struct RotationClip : IBufferElementData
    {
        public float StartTime;
        public float Duration;

        public float3 StartEuler;
        public float3 EndEuler;

        public Ease Easing;
        public float EndTime => StartTime + Duration;
    }

    public struct RotationTimelineState : IComponentData, IEnableableComponent
    {
        public float CurrentTime;
        public float PlaybackSpeed;
        public int CurrentClipIndex;
        public int Loops;
        public bool IsPlaying;
    }

    public struct RotationOrigin : IComponentData
    {
        public quaternion Value;
        public bool IsCaptured;
    }
}