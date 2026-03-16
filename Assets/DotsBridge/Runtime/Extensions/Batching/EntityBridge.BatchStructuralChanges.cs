using System;
using System.Runtime.CompilerServices;
using Unity.Entities;


namespace DotsBridge
{
    public static partial class EntityBridge
    {/// <summary>
     /// Добавляет компонент всем сущностям в батче (Structural Change).
     /// Скорость (Deferred): Быстро. Около 10 000 - 50 000 объектов без просадки FPS.
     /// Скорость (Immediate): Очень медленно (остановка потоков и перестроение памяти). Не использовать в Update!
     /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity AddComponent<T>(this ListEntity batch, ApplyMode mode = ApplyMode.Deferred) where T : unmanaged, IComponentData
        {
            if (batch.Entities.IsEmpty) return batch;

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

            return batch;
        }

        /// <summary>
        /// Удаляет компонент у всех сущностей в батче (Structural Change).
        /// Скорость (Deferred): Быстро. Около 10 000 - 50 000 объектов без просадки FPS.
        /// Скорость (Immediate): Очень медленно (остановка потоков и перестроение памяти). Не использовать в Update!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity RemoveComponent<T>(this ListEntity batch, ApplyMode mode = ApplyMode.Deferred) where T : unmanaged, IComponentData
        {
            if (batch.Entities.IsEmpty) return batch;

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

            return batch;
        }

        /// <summary>
        /// Переключает состояние IEnableableComponent. Не вызывает структурных изменений.
        /// Скорость (Deferred): Сверхбыстро (переключение битовой маски). 100 000+ объектов.
        /// Скорость (Immediate): Вызывает Sync Point. Рекомендуется только для событий.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity SetEnabled<T>(this ListEntity batch, bool isEnabled, ApplyMode mode = ApplyMode.Deferred)
            where T : unmanaged, IComponentData, IEnableableComponent
        {
            if (batch.Entities.IsEmpty) return batch;

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

            return batch;
        }

        /// <summary>
        /// Включает компонент (IEnableableComponent).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity EnableComponent<T>(this ListEntity batch, ApplyMode mode = ApplyMode.Deferred)
            where T : unmanaged, IComponentData, IEnableableComponent
        {
            return batch.SetEnabled<T>(true, mode);
        }

        /// <summary>
        /// Выключает компонент (IEnableableComponent).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity DisableComponent<T>(this ListEntity batch, ApplyMode mode = ApplyMode.Deferred)
            where T : unmanaged, IComponentData, IEnableableComponent
        {
            return batch.SetEnabled<T>(false, mode);
        }

        public delegate void RefAction<T>(Entity entity, ref T component);

        /// <summary>
        /// Безопасно перебирает сущности для чтения. Защищает от утечек памяти.
        /// Скорость: Быстро (C# цикл). Вызывает Sync Point.
        /// Лимит: Около 10 000 - 50 000 объектов. Идеально для событий и UI. Не использовать для тяжелой математики каждый кадр.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEach<T>(this ListEntity batch, Action<Entity, T> action) where T : unmanaged, IComponentData
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

        /// <summary>
        /// Позволяет прочитать и мгновенно (или отложенно) изменить компонент по ссылке.
        /// Скорость: Быстро (0 аллокаций памяти), но вызывает Sync Point.
        /// Лимит: ~10 000 объектов. Идеально для разового применения урона, баффов или лечения.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity Modify<T>(this ListEntity batch, RefAction<T> action, ApplyMode mode = ApplyMode.Deferred)
            where T : unmanaged, IComponentData
        {
            if (batch.Entities.IsEmpty) return batch;

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

            return batch;
        }
    }
}
