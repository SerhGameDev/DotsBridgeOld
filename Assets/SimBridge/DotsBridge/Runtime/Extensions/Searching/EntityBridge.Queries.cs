using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;


namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Мгновенный доступ к группе. О(1). 0 аллокаций.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity GetEntitiesFromContainer(this BridgeWorld bridge, string groupName)
        {
            if (bridge.Groups.TryGetValue(groupName.GetHashCode(), out var list))
                return new ListEntity(bridge, list, isOwner: false);

            return new ListEntity(bridge, Allocator.Temp);
        }

        /// <summary>
        /// Запрос к миру. Использует нативный буфер. 
        /// </summary>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity GetEntitiesFromContainer(this BridgeWorld bridge, params ComponentType[] componentTypes)
        {
            var batch = new ListEntity(bridge, Allocator.Temp);

            var query = bridge.Manager.CreateEntityQuery(componentTypes);

            using var entityArray = query.ToEntityArray(Allocator.Temp);
            batch.Entities.AddRange(entityArray);

            return batch;
        }

        /// <summary>
        /// Добавляет в текущий батч все сущности из указанной группы BridgeWorld.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity AddEntitiesFromContainer(this ListEntity batch, string groupName)
        {
            if (!batch.Entities.IsCreated) return batch;

            int hash = GetHash(groupName);
            if (batch.Word.Groups.TryGetValue(hash, out var sourceList))
            {
                batch.Entities.AddRange(sourceList.AsArray());
            }

            return batch;
        }

        /// <summary>
        /// Находит сущности по компонентам и добавляет их в текущий батч.
        /// </summary>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity AddEntitiesFromQuery(this ListEntity batch, params ComponentType[] componentTypes)
        {
            if (!batch.Entities.IsCreated || componentTypes == null || componentTypes.Length == 0)
                return batch;

            var queryDesc = new EntityQueryDesc { All = componentTypes };
            var query = batch.Word.Manager.CreateEntityQuery(queryDesc);

            using (var entityArray = query.ToEntityArray(Allocator.Temp))
            {
                batch.Entities.AddRange(entityArray);
            }

            return batch;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ListEntity GetByQuery(BridgeWorld bridge, params ComponentType[] types)
        {
            if (bridge == null) return new ListEntity(null, Allocator.Temp);
            return bridge.GetEntitiesFromContainer(types);
        }

        public static ListEntity GetFromContainer<T1>(this BridgeWorld b) where T1 : struct, IComponentData
            => GetByQuery(b, typeof(T1));

        public static ListEntity GetFromContainer<T1, T2>(this BridgeWorld b)
            where T1 : struct, IComponentData where T2 : struct, IComponentData
            => GetByQuery(b, typeof(T1), typeof(T2));

        public static ListEntity GetFromContainer<T1, T2, T3>(this BridgeWorld b)
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData
            => GetByQuery(b, typeof(T1), typeof(T2), typeof(T3));

        public static ListEntity GetFromContainer<T1, T2, T3, T4>(this BridgeWorld b)
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData where T4 : struct, IComponentData
            => GetByQuery(b, typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        public static ListEntity GetFromContainer<T1, T2, T3, T4, T5>(this BridgeWorld b)
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData where T4 : struct, IComponentData where T5 : struct, IComponentData
            => GetByQuery(b, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

        public static ListEntity GetFromContainer<T1, T2, T3, T4, T5, T6>(this BridgeWorld b)
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData where T4 : struct, IComponentData where T5 : struct, IComponentData where T6 : struct, IComponentData
            => GetByQuery(b, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));


    }
}
