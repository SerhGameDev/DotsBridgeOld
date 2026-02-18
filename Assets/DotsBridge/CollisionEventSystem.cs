using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace DotsBridge
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct CollisionEventSystem : ISystem
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
            var collisionQueue = new NativeQueue<CollisionEventData>(Allocator.TempJob);

            var idLookup = SystemAPI.GetComponentLookup<ID>(true);

            var job = new CollectCollisionsJob
            {
                CollisionQueue = collisionQueue.AsParallelWriter(),
                IDLookup = idLookup
            };

            state.Dependency = job.Schedule(simulation, state.Dependency);
            state.Dependency.Complete();

            while (collisionQueue.TryDequeue(out CollisionEventData data))
            {
                if (Dots.CollisionEvents.TryGetValue(data.SourceID, out var action))
                {
                    action?.Invoke(data.SourceEntity, data.TargetEntity);
                }
            }

            collisionQueue.Dispose();
        }

        private struct CollisionEventData
        {
            public int SourceID;
            public Entity SourceEntity;
            public Entity TargetEntity;
        }

        [BurstCompile]
        private struct CollectCollisionsJob : ITriggerEventsJob
        {
            public NativeQueue<CollisionEventData>.ParallelWriter CollisionQueue;
            [ReadOnly] public ComponentLookup<ID> IDLookup;

            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;

                if (IDLookup.HasComponent(entityA))
                {
                    CollisionQueue.Enqueue(new CollisionEventData
                    {
                        SourceID = IDLookup[entityA].Value,
                        SourceEntity = entityA,
                        TargetEntity = entityB 
                    });
                }

                if (IDLookup.HasComponent(entityB))
                {
                    CollisionQueue.Enqueue(new CollisionEventData
                    {
                        SourceID = IDLookup[entityB].Value,
                        SourceEntity = entityB,
                        TargetEntity = entityA
                    });
                }
            }
        }
    }
}