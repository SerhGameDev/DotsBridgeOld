using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        [BurstCompile]
        private struct SetDataImmediateJob<T> : IJob where T : unmanaged, IComponentData
        {
            [ReadOnly] public NativeArray<Entity> Entities;
            public ComponentLookup<T> Lookup;
            public T Data;

            public void Execute()
            {
                for (int i = 0; i < Entities.Length; i++)
                {
                    Entity entity = Entities[i];
                    if (Lookup.HasComponent(entity))
                    {
                        Lookup[entity] = Data;
                    }
                }
            }
        }

        [BurstCompile]
        private struct GetDataImmediateJob<T> : IJob where T : unmanaged, IComponentData
        {
            [ReadOnly] public NativeArray<Entity> Entities;
            [ReadOnly] public ComponentLookup<T> Lookup;
            [WriteOnly] public NativeArray<T> Results;

            public void Execute()
            {
                for (int i = 0; i < Entities.Length; i++)
                {
                    Entity entity = Entities[i];
                    if (Lookup.HasComponent(entity))
                    {
                        Results[i] = Lookup[entity];
                    }
                    else
                    {
                        Results[i] = default;
                    }
                }
            }
        }
    }

}