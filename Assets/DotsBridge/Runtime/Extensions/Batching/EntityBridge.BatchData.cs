using DotsBridge;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Получает массив данных компонента для всех сущностей в батче.
        /// ВНИМАНИЕ: Обязательно вызовите Dispose() у результата для избежания утечек памяти!
        /// Скорость: Высокая (Burst), но вызывает Sync Point.
        /// Лимит: Легко обрабатывает 100 000+ объектов за раз. Не вызывать чаще 1-3 раз за кадр.
        /// </summary>
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

        /// <summary>
        /// Получает данные только первой сущности в батче (удобно для синглтонов).
        /// Скорость: Молниеносно (O(1)), но вызывает Sync Point.
        /// Лимит: Использовать для разовых проверок состояний.
        /// </summary>
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

        /// <summary>
        /// Устанавливает значения компонента для всех сущностей в батче.
        /// Скорость (Deferred): Сверхбыстро. Выдерживает 200 000+ объектов каждый кадр.
        /// Скорость (Immediate): Быстро (Burst), но вызывает Sync Point. Не более 2-5 вызовов за кадр.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch SetData<T>(this EntityBatch batch, T data, ApplyMode mode = ApplyMode.Deferred) where T : unmanaged, IComponentData
        {
            if (batch.Entities.IsEmpty) return batch;

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

            return batch;
        }
    }
}