using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using DotsBridge.Timeline;

namespace DotsBridge.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
    public partial struct RelativeTimelineSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var job = new TimelineJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                ECB = ecb 
            };

            job.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct TimelineJob : IJobEntity
        {
            public float DeltaTime;
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(
                ref LocalTransform transform,
                ref TimelineState tl,
                ref SequenceOrigin origin,
                in DynamicBuffer<RelativeMoveClip> buffer,
                Entity entity,
                [ChunkIndexInQuery] int chunkIndex) 
            {
                if (!origin.IsCaptured)
                {
                    origin.Value = transform.Position;
                    origin.IsCaptured = true;
                }

                tl.CurrentTime += DeltaTime * tl.PlaybackSpeed;
                bool processed = false;

                while (tl.CurrentClipIndex < buffer.Length)
                {
                    RelativeMoveClip clip = buffer[tl.CurrentClipIndex];

                    if (tl.CurrentTime > clip.EndTime)
                    {
                        transform.Position = origin.Value + clip.EndOffset;
                        tl.CurrentClipIndex++;
                        continue;
                    }

                    if (tl.CurrentTime >= clip.StartTime)
                    {
                        float t = (tl.CurrentTime - clip.StartTime) / math.max(clip.Duration, 0.001f);
                        float easedT = ApplyEasing(t, clip.Easing);
                        transform.Position = origin.Value + math.lerp(clip.StartOffset, clip.EndOffset, easedT);
                        processed = true;
                        break;
                    }

                    if (tl.CurrentTime < clip.StartTime)
                    {
                        processed = true;
                        break;
                    }
                }

                if (!processed && tl.CurrentClipIndex >= buffer.Length)
                {
                    if (tl.Loops == -1 || tl.Loops > 0)
                    {
                        if (tl.Loops > 0) tl.Loops--;

                        tl.CurrentTime = 0;       
                        tl.CurrentClipIndex = 0; 
                    }
                    else
                    {
                        tl.IsPlaying = false;
                        ECB.SetComponentEnabled<TimelineState>(chunkIndex, entity, false);
                    }
                }
            }

            private static float ApplyEasing(float t, Ease ease)
            {
                t = math.clamp(t, 0, 1);
                switch (ease)
                {
                    case Ease.InQuad: return t * t;
                    case Ease.OutQuad: return t * (2 - t);
                    case Ease.InOutQuad: return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
                    case Ease.InSine: return 1 - math.cos((t * math.PI) / 2);
                    case Ease.OutSine: return math.sin((t * math.PI) / 2);
                    case Ease.InOutSine: return -(math.cos(math.PI * t) - 1) / 2;
                    case Ease.InExp: return t == 0 ? 0 : math.pow(2, 10 * t - 10);
                    case Ease.OutExp: return t == 1 ? 1 : 1 - math.pow(2, -10 * t);
                    default: return t;
                }
            }
        }
    }
}