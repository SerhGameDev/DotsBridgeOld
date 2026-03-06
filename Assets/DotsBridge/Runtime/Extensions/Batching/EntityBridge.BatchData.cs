using DotsBridge;
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> GetData<T>(this EntityBatch batch, Allocator allocator = Allocator.Temp) where T : unmanaged, IComponentData
        {
            if (batch.Entities.IsEmpty) return new NativeArray<T>(0, allocator);

            var unmanagedWorld = batch.Manager.World.Unmanaged;
            SystemHandle bridgeSystem = unmanagedWorld.GetExistingUnmanagedSystem<DotsBridgeSystem>();

            if (bridgeSystem == SystemHandle.Null)
                bridgeSystem = batch.Manager.World.CreateSystem<DotsBridgeSystem>();

            ref SystemState state = ref unmanagedWorld.ResolveSystemStateRef(bridgeSystem);

            var query = batch.Manager.CreateEntityQuery(ComponentType.ReadOnly<T>());
            query.CompleteDependency();

            var lookup = state.GetComponentLookup<T>(true);
            lookup.Update(ref state);

            var results = new NativeArray<T>(batch.Entities.Length, allocator);

            new GetDataImmediateJob<T>
            {
                Entities = batch.Entities.AsArray(),
                Lookup = lookup,
                Results = results
            }.Run();

            return results;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetFirstData<T>(this EntityBatch batch) where T : unmanaged, IComponentData
        {
            if (batch.Entities.IsEmpty) return default;

            var query = batch.Manager.CreateEntityQuery(ComponentType.ReadOnly<T>());
            query.CompleteDependency();

            Entity firstEntity = batch.Entities[0];

            if (batch.Manager.HasComponent<T>(firstEntity))
            {
                return batch.Manager.GetComponentData<T>(firstEntity);
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEach<T>(this EntityBatch batch, Action<Entity, T> action) where T : unmanaged, IComponentData
        {
            if (batch.Entities.IsEmpty) return;

            var unmanagedWorld = batch.Manager.World.Unmanaged;
            SystemHandle bridgeSystem = unmanagedWorld.GetExistingUnmanagedSystem<DotsBridgeSystem>();

            if (bridgeSystem == SystemHandle.Null)
                bridgeSystem = batch.Manager.World.CreateSystem<DotsBridgeSystem>();

            ref SystemState state = ref unmanagedWorld.ResolveSystemStateRef(bridgeSystem);

            var query = batch.Manager.CreateEntityQuery(ComponentType.ReadOnly<T>());
            query.CompleteDependency();

            var lookup = state.GetComponentLookup<T>(true);
            lookup.Update(ref state);

            var entities = batch.Entities.AsArray();

            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                if (lookup.HasComponent(entity))
                {
                    action(entity, lookup[entity]);
                }
            }
        }
    }
}