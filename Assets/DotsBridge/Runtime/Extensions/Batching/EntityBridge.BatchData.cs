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
        /// Получает данные только первой сущности в батче.
        /// Оптимизировано: больше не создает временные Query.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetFirst<T>(this EntityBatch batch, BridgeRegistry registry) where T : unmanaged, IComponentData
        {
            if (batch.IsEmpty()) return default;

            Entity firstEntity = batch.Entities[0];

            // Используем менеджер того мира, к которому принадлежит батч
            if (registry.Manager.HasComponent<T>(firstEntity))
            {
                return registry.Manager.GetComponentData<T>(firstEntity);
            }

            return default;
        }
        // =========================================================
        // SERVER GETTERS
        // =========================================================

        /// <summary> Находит сущность по ID на сервере и возвращает её компонент. </summary>
        public static T GetFirstForServer<T>(string id) where T : unmanaged, IComponentData
        {
            var batch = GetForServer(id); // Используем наш O(1) поиск по ID
            return batch.GetFirst<T>(ServerRegistry);
        }

        // =========================================================
        // CLIENT GETTERS
        // =========================================================

        /// <summary> Находит сущность по ID на клиенте и возвращает её компонент. </summary>
        public static T GetFirstForClient<T>(string id) where T : unmanaged, IComponentData
        {
            var batch = GetClient(id);
            return batch.GetFirst<T>(ClientRegistry);
        }

        /// <summary> Находит группу по тегам на клиенте и берет компонент у первой сущности. </summary>
        public static T GetFirstForClient<T>(params ComponentType[] tag) where T : unmanaged, IComponentData
        {
            using var batch = GetForClient(tag);
            return batch.GetFirst<T>(ClientRegistry);
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