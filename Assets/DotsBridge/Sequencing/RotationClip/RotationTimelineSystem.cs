using DotsBridge.Timeline;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct RotationTimelineSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        var job = new RotationJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        };
        job.ScheduleParallel();
    }

    [BurstCompile]
    public partial struct RotationJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute(
            ref LocalTransform transform,
            ref RotationTimelineState tl,
            ref RotationOrigin origin,
            in DynamicBuffer<RotationClip> buffer,
            Entity entity,
            [ChunkIndexInQuery] int chunkIndex)
        {
            if (!origin.IsCaptured)
            {
                origin.Value = transform.Rotation;
                origin.IsCaptured = true;
            }

            tl.CurrentTime += DeltaTime * tl.PlaybackSpeed;
            bool processed = false;

            while (tl.CurrentClipIndex < buffer.Length)
            {
                RotationClip clip = buffer[tl.CurrentClipIndex];

                if (tl.CurrentTime > clip.EndTime)
                {
                    quaternion endRot = quaternion.Euler(math.radians(clip.EndEuler));
                    transform.Rotation = math.mul(origin.Value, endRot);

                    tl.CurrentClipIndex++;
                    continue;
                }

                if (tl.CurrentTime >= clip.StartTime)
                {
                    float t = (tl.CurrentTime - clip.StartTime) / math.max(clip.Duration, 0.001f);
                    float easedT = ApplyEasing(t, clip.Easing);

                    float3 currentEuler = math.lerp(clip.StartEuler, clip.EndEuler, easedT);

                    quaternion deltaRot = quaternion.Euler(math.radians(currentEuler));
                    transform.Rotation = math.mul(origin.Value, deltaRot);

                    processed = true;
                    break;
                }

                if (tl.CurrentTime < clip.StartTime) { processed = true; break; }
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
                    ECB.SetComponentEnabled<RotationTimelineState>(chunkIndex, entity, false);
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