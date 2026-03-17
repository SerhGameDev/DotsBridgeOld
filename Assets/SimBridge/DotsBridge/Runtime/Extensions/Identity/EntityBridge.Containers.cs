using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {

        /// <summary>
        /// Динамически собирает новый список сущностей, используя IdMapSystem.
        /// 
        /// КОГДА ИСПОЛЬЗОВАТЬ:
        /// - Для поиска конкретных уникальных объектов (например, "Босс_Уровня_1").
        /// - Когда сущности часто создаются/удаляются и вы не хотите следить за списками вручную.
        /// 
        /// МИНУСЫ:
        /// - Выделяет память под новый NativeList (требует вызова ApplyAndDispose() в конце).
        /// - Зависит от того, успела ли IdMapSystem обновиться в этом кадре.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity FindById(this BridgeWorld bridge, string id, Allocator allocator = Allocator.Temp)
        {
            var list = new NativeList<Entity>(allocator);
            FillListFromMap(bridge, GetHash(id), ref list);
            return new ListEntity(bridge, list, isOwner: true);
        }
        /// <summary>
        /// Создает новый батч (ListEntity), собирая все сущности в мире, у которых есть указанный компонент T.
        /// Выполняется синхронно через EntityQuery.
        /// 
        /// ВНИМАНИЕ: Выделяет память под новый NativeList (требует вызова Dispose() у ListEntity).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity FindWithComponent<T>(this BridgeWorld bridge, Allocator allocator = Allocator.Temp)
            where T : unmanaged, IComponentData
        {
            // Создаем запрос к ECS на поиск всех сущностей с компонентом T
            var query = bridge.Manager.CreateEntityQuery(ComponentType.ReadOnly<T>());

            // Получаем массив сущностей (Temp аллокатор, чтобы сразу очистить массив)
            using var entityArray = query.ToEntityArray(Allocator.Temp);

            // Перекладываем в NativeList, который ожидает ваша структура ListEntity
            var list = new NativeList<Entity>(entityArray.Length, allocator);
            list.AddRange(entityArray);

            return new ListEntity(bridge, list, isOwner: true);
        }

        /// <summary>
        /// Создает новый батч (ListEntity), собирая сущности, у которых есть ОБА указанных компонента (T1 и T2).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity FindWithComponent<T1, T2>(this BridgeWorld bridge, Allocator allocator = Allocator.Temp)
            where T1 : unmanaged, IComponentData
            where T2 : unmanaged, IComponentData
        {
            var query = bridge.Manager.CreateEntityQuery(
                ComponentType.ReadOnly<T1>(),
                ComponentType.ReadOnly<T2>()
            );

            using var entityArray = query.ToEntityArray(Allocator.Temp);

            var list = new NativeList<Entity>(entityArray.Length, allocator);
            list.AddRange(entityArray);

            return new ListEntity(bridge, list, isOwner: true);
        }

        /// <summary>
        /// Создает новый батч (ListEntity), собирая сущности, у которых есть ВСЕ ТРИ указанных компонента.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity FindWithComponent<T1, T2, T3>(this BridgeWorld bridge, Allocator allocator = Allocator.Temp)
            where T1 : unmanaged, IComponentData
            where T2 : unmanaged, IComponentData
            where T3 : unmanaged, IComponentData
        {
            var query = bridge.Manager.CreateEntityQuery(
                ComponentType.ReadOnly<T1>(),
                ComponentType.ReadOnly<T2>(),
                ComponentType.ReadOnly<T3>()
            );

            using var entityArray = query.ToEntityArray(Allocator.Temp);

            var list = new NativeList<Entity>(entityArray.Length, allocator);
            list.AddRange(entityArray);

            return new ListEntity(bridge, list, isOwner: true);
        }
        /// <summary>
        /// Добавляет сущности, найденные через IdMapSystem, в существующий батч.
        /// </summary>
        public static ListEntity AddFoundById(this ListEntity batch, string id)
        {
            if (!batch.Entities.IsCreated) return batch;
            FillListFromMap(batch.Word, GetHash(id), batch.Entities);
            return batch;
        }

        // =========================================================
        // ВНУТРЕННЯЯ ЛОГИКА
        // =========================================================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillListFromMap(BridgeWorld bridge, int idHash, ref NativeList<Entity> results)
        {
            var systemHandle = bridge.World.GetExistingSystem<IdMapSystem>();
            if (systemHandle == SystemHandle.Null) return;

            var map = bridge.World.Unmanaged.GetUnsafeSystemRef<IdMapSystem>(systemHandle).EntityMap;

            if (map.IsCreated && map.TryGetFirstValue(idHash, out Entity entity, out var iterator))
            {
                do
                {
                    if (bridge.Manager.Exists(entity))
                    {
                        results.Add(entity);
                    }
                }
                while (map.TryGetNextValue(out entity, ref iterator));
            }
        }

        /// <summary>
        /// Находит сущности по ID и добавляет их в ТЕКУЩИЙ батч.
        /// Удобно для сбора нескольких ID в одну группу.
        /// </summary>
        public static ListEntity AndAddById(this ListEntity batch, string id)
        {
            if (!batch.Entities.IsCreated) return batch;

            FillListFromMap(batch.Word, GetHash(id), batch.Entities);
            return batch;
        }

        /// <summary>
        /// Оставляет в текущем батче ТОЛЬКО сущности с указанным ID.
        /// </summary>
        public static ListEntity FindWithId(this ListEntity batch, string id)
        {
            if (!batch.Entities.IsCreated || batch.Entities.Length == 0) return batch;
            return batch.Filter(new IdFilter { TargetHash = GetHash(id) });
        }


        private struct IdFilter : IEntityFilter
        {
            public int TargetHash;
            public bool Execute(SingleEntity entity)
            {
                return entity.HasComponent<EntityIdComponent>() &&
                       entity.GetComponent<EntityIdComponent>().Hash == TargetHash;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillListFromMap(BridgeWorld bridge, int idHash, NativeList<Entity> results)
        {
            var systemHandle = bridge.World.GetExistingSystem<IdMapSystem>();
            if (systemHandle == SystemHandle.Null) return;

            var map = bridge.World.Unmanaged.GetUnsafeSystemRef<IdMapSystem>(systemHandle).EntityMap;

            if (map.IsCreated && map.TryGetFirstValue(idHash, out Entity entity, out var iterator))
            {
                do
                {
                    if (bridge.Manager.Exists(entity))
                    {
                        results.Add(entity);
                    }
                }
                while (map.TryGetNextValue(out entity, ref iterator));
            }
        }
    }
}
