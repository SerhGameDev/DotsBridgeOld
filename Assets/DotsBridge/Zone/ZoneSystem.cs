using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace DotsBridge
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct ZoneSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var queue = new NativeQueue<ZoneEvent>(Allocator.TempJob);

            var job = new ZoneTriggerJob
            {
                Queue = queue.AsParallelWriter(),
                ZoneLookup = SystemAPI.GetComponentLookup<ZoneComponent>(true)
            };

            state.Dependency = job.Schedule(simulation, state.Dependency);
            state.Dependency.Complete();

            while (queue.TryDequeue(out ZoneEvent evt))
            {
                if (ZoneBase.ActiveZones.TryGetValue(evt.ZoneInstanceID, out var zone))
                {
                    try
                    {
                        zone.OnZoneStay(evt.Intruder);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }

            queue.Dispose();
        }

        private struct ZoneEvent
        {
            public int ZoneInstanceID;
            public Entity Intruder;
        }

        [BurstCompile]
        private struct ZoneTriggerJob : ITriggerEventsJob
        {
            public NativeQueue<ZoneEvent>.ParallelWriter Queue;
            [ReadOnly] public ComponentLookup<ZoneComponent> ZoneLookup;

            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;

                if (ZoneLookup.HasComponent(entityA))
                {
                    Queue.Enqueue(new ZoneEvent
                    {
                        ZoneInstanceID = ZoneLookup[entityA].InstanceID,
                        Intruder = entityB
                    });
                }

                if (ZoneLookup.HasComponent(entityB))
                {
                    Queue.Enqueue(new ZoneEvent
                    {
                        ZoneInstanceID = ZoneLookup[entityB].InstanceID,
                        Intruder = entityA
                    });
                }
            }
        }
    }
}