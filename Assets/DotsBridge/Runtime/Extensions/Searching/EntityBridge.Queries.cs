using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;


namespace DotsBridge
{
    public static partial class EntityBridge
    {
        // =========================================================
        // БАЗОВЫЕ МЕТОДЫ (SERVER / CLIENT)
        // =========================================================

        public static EntityBatch GetForServer(params ComponentType[] componentTypes) => Get(ServerRegistry, componentTypes);
        public static EntityBatch GetForClient(params ComponentType[] componentTypes) => Get(ClientRegistry, componentTypes);

        /// <summary>
        /// Приватный базовый метод. Выполняет поиск в конкретном реестре.
        /// </summary>
        private static EntityBatch Get(BridgeRegistry registry, params ComponentType[] componentTypes)
        {
            var targetRegistry = registry ?? GetActiveRegistry();

            if (targetRegistry == null)
                throw new ArgumentException("[DotsBridge] Не найден активный Registry для выполнения Get!");

            if (componentTypes == null || componentTypes.Length == 0)
                throw new ArgumentException("Укажите хотя бы один компонент для поиска.");

            var queryDesc = new EntityQueryDesc { All = componentTypes };
            var query = targetRegistry.Manager.CreateEntityQuery(queryDesc);

            // Получаем сущности из конкретного менеджера мира
            var entityArray = query.ToEntityArray(Allocator.Temp);

            var entityList = new NativeList<Entity>(entityArray.Length, Allocator.Persistent);
            entityList.AddRange(entityArray);

            entityArray.Dispose();
            // Query не диспозим, так как они кэшируются в EntityManager

            return new EntityBatch(entityList, Manager);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetForServer<T1>() where T1 : struct, IComponentData
            => Get(ServerRegistry, ComponentType.ReadOnly<T1>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetForServer<T1, T2>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData
            => Get(ServerRegistry, ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetForServer<T1, T2, T3>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData
            => Get(ServerRegistry, ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>(), ComponentType.ReadOnly<T3>());


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetForClient<T1>() where T1 : struct, IComponentData
            => Get(ClientRegistry, ComponentType.ReadOnly<T1>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetForClient<T1, T2>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData
            => Get(ClientRegistry, ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetForClient<T1, T2, T3>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData where T3 : struct, IComponentData
            => Get(ClientRegistry, ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>(), ComponentType.ReadOnly<T3>());


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetForServer<T1, T2, T3, T4>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData
            where T3 : struct, IComponentData where T4 : struct, IComponentData
            => Get(ServerRegistry, ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>(),
                   ComponentType.ReadOnly<T3>(), ComponentType.ReadOnly<T4>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetForServer<T1, T2, T3, T4, T5>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData
            where T3 : struct, IComponentData where T4 : struct, IComponentData
            where T5 : struct, IComponentData
            => Get(ServerRegistry, ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>(),
                   ComponentType.ReadOnly<T3>(), ComponentType.ReadOnly<T4>(), ComponentType.ReadOnly<T5>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetForServer<T1, T2, T3, T4, T5, T6>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData
            where T3 : struct, IComponentData where T4 : struct, IComponentData
            where T5 : struct, IComponentData where T6 : struct, IComponentData
            => Get(ServerRegistry, ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>(),
                   ComponentType.ReadOnly<T3>(), ComponentType.ReadOnly<T4>(),
                   ComponentType.ReadOnly<T5>(), ComponentType.ReadOnly<T6>());


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetForClient<T1, T2, T3, T4>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData
            where T3 : struct, IComponentData where T4 : struct, IComponentData
            => Get(ClientRegistry, ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>(),
                   ComponentType.ReadOnly<T3>(), ComponentType.ReadOnly<T4>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetForClient<T1, T2, T3, T4, T5>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData
            where T3 : struct, IComponentData where T4 : struct, IComponentData
            where T5 : struct, IComponentData
            => Get(ClientRegistry, ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>(),
                   ComponentType.ReadOnly<T3>(), ComponentType.ReadOnly<T4>(), ComponentType.ReadOnly<T5>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetForClient<T1, T2, T3, T4, T5, T6>()
            where T1 : struct, IComponentData where T2 : struct, IComponentData
            where T3 : struct, IComponentData where T4 : struct, IComponentData
            where T5 : struct, IComponentData where T6 : struct, IComponentData
            => Get(ClientRegistry, ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>(),
                   ComponentType.ReadOnly<T3>(), ComponentType.ReadOnly<T4>(),
                   ComponentType.ReadOnly<T5>(), ComponentType.ReadOnly<T6>());

    }
}
