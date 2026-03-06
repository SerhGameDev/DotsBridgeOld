using System;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    /// <summary>
    /// Безопасный маршализатор запросов (Query).
    /// Отвечает за роутинг между мирами и автоматическое управление памятью.
    /// </summary>
    public struct DotsQuery
    {
        private readonly BridgeState _state;
        private Func<BridgeState, EntityBatch> _targetResolver;
        private bool _requiresDispose;

        public DotsQuery(BridgeState state)
        {
            _state = state ?? EntityBridge.GetActiveState();
            _targetResolver = null;
            _requiresDispose = false;
        }

        // =========================================================
        // ВНУТРЕННЕЕ УПРАВЛЕНИЕ (Для экстеншенов)
        // =========================================================

        internal DotsQuery SetInternalResolver(Func<BridgeState, EntityBatch> resolver, bool requiresDispose)
        {
            this._targetResolver = resolver;
            this._requiresDispose = requiresDispose;
            return this;
        }

        // =========================================================
        // РОУТИНГ (ВЫБОР ЦЕЛИ)
        // =========================================================

        /// <summary> Выбор конкретной группы сущностей по строковому ID. </summary>
        public DotsQuery GetById(string id)
        {
            _targetResolver = (s) => EntityBridge.GetByIdInternal(s, id);
            _requiresDispose = false; // Контейнеры ID живут вечно, их нельзя удалять
            return this;
        }

        // =========================================================
        // БЕЗОПАСНАЯ ПЕСОЧНИЦА (RUN)
        // =========================================================

        /// <summary> Выполняет логику чтения, возвращает результат и чистит временную память. </summary>
        public T Run<T>(Func<EntityBatch, T> readLogic)
        {
            if (_targetResolver == null || _state == null) return default;

            EntityBatch batch = _targetResolver.Invoke(_state);
            T result = readLogic(batch);

            // Если батч был временным (создан через Get<T>), удаляем его NativeList
            if (_requiresDispose && batch.Entities.IsCreated)
                batch.Entities.Dispose();

            return result;
        }

        /// <summary> Выполняет логику чтения без возврата значения и чистит временную память. </summary>
        public void Run(Action<EntityBatch> readLogic)
        {
            if (_targetResolver == null || _state == null) return;

            EntityBatch batch = _targetResolver.Invoke(_state);
            readLogic(batch);

            if (_requiresDispose && batch.Entities.IsCreated)
                batch.Entities.Dispose();
        }
    }

    /// <summary>
    /// Методы расширения для DotsQuery. Сюда можно добавлять любую логику чтения.
    /// </summary>
    public static partial class DotsQueryExtensions
    {
        // --- ВЫБОР ЦЕЛИ ПО КОМПОНЕНТАМ ---

        public static DotsQuery Get<T1>(this DotsQuery query) where T1 : struct, IComponentData
            => query.SetInternalResolver(s => EntityBridge.GetByComponentsInternal(s, new[] { ComponentType.ReadOnly<T1>() }), true);

        public static DotsQuery Get<T1, T2>(this DotsQuery query)
            where T1 : struct, IComponentData where T2 : struct, IComponentData
            => query.SetInternalResolver(s => EntityBridge.GetByComponentsInternal(s, new[] { ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>() }), true);

        // --- МЕТОДЫ ЧТЕНИЯ ДАННЫХ ---

        /// <summary> Получает данные первого найденного компонента в выборке. </summary>
        public static T GetFirstData<T>(this DotsQuery query) where T : unmanaged, IComponentData
        {
            return query.Run(batch =>
            {
                if (batch.IsEmpty) return default;

                var entity = batch.Entities[0];
                // Для ОДНОЙ сущности можно использовать Manager напрямую, это просто и быстро
                if (batch.Manager.HasComponent<T>(entity))
                    return batch.Manager.GetComponentData<T>(entity);

                return default;
            });
        }

        /// <summary> Выполняет действие для каждой сущности в выборке (Оптимизировано через Lookup). </summary>
        public static void ForEach<T>(this DotsQuery query, Action<Entity, T> action) where T : unmanaged, IComponentData
        {
            query.Run(batch =>
            {
                if (batch.IsEmpty) return;

                // 1. Получаем доступ к системному состоянию через Unmanaged World
                var unmanagedWorld = batch.Manager.World.Unmanaged;

                // 2. Находим (или создаем) нашу системную прослойку
                SystemHandle bridgeSystem = unmanagedWorld.GetExistingUnmanagedSystem<DotsBridgeSystem>();
                if (bridgeSystem == SystemHandle.Null)
                    bridgeSystem = batch.Manager.World.CreateSystem<DotsBridgeSystem>();

                // 3. Получаем State и из него - заветный Lookup
                ref SystemState state = ref unmanagedWorld.ResolveSystemStateRef(bridgeSystem);
                var lookup = state.GetComponentLookup<T>(true); // true = ReadOnly

                // 4. Обязательно обновляем Lookup перед использованием!
                lookup.Update(ref state);

                for (int i = 0; i < batch.Count; i++)
                {
                    var entity = batch.Entities[i];
                    if (lookup.HasComponent(entity))
                        action(entity, lookup[entity]);
                }
            });
        }

        /// <summary> Копирует данные всех сущностей в NativeArray. </summary>
        public static NativeArray<T> GetDataArray<T>(this DotsQuery query, Allocator allocator = Allocator.Temp) where T : unmanaged, IComponentData
        {
            return query.Run(batch =>
            {
                if (batch.IsEmpty) return new NativeArray<T>(0, allocator);

                var result = new NativeArray<T>(batch.Count, allocator);

                // Повторяем логику получения Lookup
                var unmanagedWorld = batch.Manager.World.Unmanaged;
                SystemHandle bridgeSystem = unmanagedWorld.GetExistingUnmanagedSystem<DotsBridgeSystem>();
                ref SystemState state = ref unmanagedWorld.ResolveSystemStateRef(bridgeSystem);
                var lookup = state.GetComponentLookup<T>(true);
                lookup.Update(ref state);

                for (int i = 0; i < batch.Count; i++)
                    result[i] = lookup[batch.Entities[i]];

                return result;
            });
        }
    }
}