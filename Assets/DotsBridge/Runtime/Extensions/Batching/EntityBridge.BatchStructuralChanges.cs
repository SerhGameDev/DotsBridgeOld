using System;
using System.Runtime.CompilerServices;
using Unity.Entities;


namespace DotsBridge
{
    public static partial class EntityBridge
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DotsCommand SetData<T>(this DotsCommand cmd, T data, ApplyMode mode = ApplyMode.Deferred) where T : unmanaged, IComponentData
        {
            return cmd.Do(batch =>
            {
                if (batch.Entities.IsEmpty) return;

                if (mode == ApplyMode.Immediate)
                {
                    var unmanagedWorld = batch.Manager.World.Unmanaged;
                    SystemHandle bridgeSystem = unmanagedWorld.GetExistingUnmanagedSystem<DotsBridgeSystem>();

                    if (bridgeSystem == SystemHandle.Null)
                        bridgeSystem = batch.Manager.World.CreateSystem<DotsBridgeSystem>();

                    ref SystemState state = ref unmanagedWorld.ResolveSystemStateRef(bridgeSystem);

                    var query = batch.Manager.CreateEntityQuery(ComponentType.ReadWrite<T>());
                    query.CompleteDependency();

                    var lookup = state.GetComponentLookup<T>(false);
                    lookup.Update(ref state);

                    new SetDataImmediateJob<T>
                    {
                        Entities = batch.Entities.AsArray(),
                        Lookup = lookup,
                        Data = data
                    }.Run();
                }
                else
                {
                    var ecbSystem = batch.Manager.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
                    var ecb = ecbSystem.CreateCommandBuffer();
                    var entities = batch.Entities.AsArray();

                    for (int i = 0; i < entities.Length; i++)
                    {
                        if (batch.Manager.HasComponent<T>(entities[i]))
                            ecb.SetComponent(entities[i], data);
                    }
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DotsCommand AddComponent<T>(this DotsCommand cmd, ApplyMode mode = ApplyMode.Deferred) where T : unmanaged, IComponentData
        {
            return cmd.Do(batch =>
            {
                if (batch.Entities.IsEmpty) return;

                if (mode == ApplyMode.Immediate)
                {
                    batch.Manager.AddComponent<T>(batch.Entities.AsArray());
                }
                else
                {
                    var ecbSystem = batch.Manager.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
                    var ecb = ecbSystem.CreateCommandBuffer();
                    var entities = batch.Entities.AsArray();

                    for (int i = 0; i < entities.Length; i++)
                    {
                        ecb.AddComponent<T>(entities[i]);
                    }
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DotsCommand RemoveComponent<T>(this DotsCommand cmd, ApplyMode mode = ApplyMode.Deferred) where T : unmanaged, IComponentData
        {
            return cmd.Do(batch =>
            {
                if (batch.Entities.IsEmpty) return;

                if (mode == ApplyMode.Immediate)
                {
                    batch.Manager.RemoveComponent<T>(batch.Entities.AsArray());
                }
                else
                {
                    var ecbSystem = batch.Manager.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
                    var ecb = ecbSystem.CreateCommandBuffer();
                    var entities = batch.Entities.AsArray();

                    for (int i = 0; i < entities.Length; i++)
                    {
                        ecb.RemoveComponent<T>(entities[i]);
                    }
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DotsCommand SetEnabled<T>(this DotsCommand cmd, bool isEnabled, ApplyMode mode = ApplyMode.Deferred)
            where T : unmanaged, IComponentData, IEnableableComponent
        {
            return cmd.Do(batch =>
            {
                if (batch.Entities.IsEmpty) return;

                var entities = batch.Entities.AsArray();

                if (mode == ApplyMode.Immediate)
                {
                    var unmanagedWorld = batch.Manager.World.Unmanaged;
                    SystemHandle bridgeSystem = unmanagedWorld.GetExistingUnmanagedSystem<DotsBridgeSystem>();

                    if (bridgeSystem == SystemHandle.Null)
                        bridgeSystem = batch.Manager.World.CreateSystem<DotsBridgeSystem>();

                    ref SystemState state = ref unmanagedWorld.ResolveSystemStateRef(bridgeSystem);

                    var query = batch.Manager.CreateEntityQuery(ComponentType.ReadWrite<T>());
                    query.CompleteDependency();

                    for (int i = 0; i < entities.Length; i++)
                    {
                        batch.Manager.SetComponentEnabled<T>(entities[i], isEnabled);
                    }
                }
                else
                {
                    var ecbSystem = batch.Manager.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
                    var ecb = ecbSystem.CreateCommandBuffer();

                    for (int i = 0; i < entities.Length; i++)
                    {
                        ecb.SetComponentEnabled<T>(entities[i], isEnabled);
                    }
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DotsCommand EnableComponent<T>(this DotsCommand cmd, ApplyMode mode = ApplyMode.Deferred)
            where T : unmanaged, IComponentData, IEnableableComponent
        {
            return cmd.SetEnabled<T>(true, mode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DotsCommand DisableComponent<T>(this DotsCommand cmd, ApplyMode mode = ApplyMode.Deferred)
            where T : unmanaged, IComponentData, IEnableableComponent
        {
            return cmd.SetEnabled<T>(false, mode);
        }

        public delegate void RefAction<T>(Entity entity, ref T component);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DotsCommand Modify<T>(this DotsCommand cmd, RefAction<T> action, ApplyMode mode = ApplyMode.Deferred)
            where T : unmanaged, IComponentData
        {
            return cmd.Do(batch =>
            {
                if (batch.Entities.IsEmpty) return;

                var unmanagedWorld = batch.Manager.World.Unmanaged;
                SystemHandle bridgeSystem = unmanagedWorld.GetExistingUnmanagedSystem<DotsBridgeSystem>();

                if (bridgeSystem == SystemHandle.Null)
                    bridgeSystem = batch.Manager.World.CreateSystem<DotsBridgeSystem>();

                ref SystemState state = ref unmanagedWorld.ResolveSystemStateRef(bridgeSystem);

                var query = batch.Manager.CreateEntityQuery(ComponentType.ReadWrite<T>());
                query.CompleteDependency();

                var lookup = state.GetComponentLookup<T>(false);
                lookup.Update(ref state);

                var entities = batch.Entities.AsArray();

                if (mode == ApplyMode.Immediate)
                {
                    for (int i = 0; i < entities.Length; i++)
                    {
                        Entity entity = entities[i];
                        if (lookup.HasComponent(entity))
                        {
                            T component = lookup[entity];
                            action(entity, ref component);
                            lookup[entity] = component;
                        }
                    }
                }
                else
                {
                    var ecbSystem = batch.Manager.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
                    var ecb = ecbSystem.CreateCommandBuffer();

                    for (int i = 0; i < entities.Length; i++)
                    {
                        Entity entity = entities[i];
                        if (lookup.HasComponent(entity))
                        {
                            T component = lookup[entity];
                            action(entity, ref component);
                            ecb.SetComponent(entity, component);
                        }
                    }
                }
            });
        }
    }
}
