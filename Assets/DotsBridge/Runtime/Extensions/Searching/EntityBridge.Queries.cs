using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;


namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Базовый метод. Получает сущности, содержащие ВСЕ указанные типы компонентов.
        /// ВНИМАНИЕ: Обязательно вызовите Dispose() у батча для избежания утечек памяти!
        /// Скорость: Средняя (Создает EntityQuery и аллоцирует NativeList).
        /// Лимит: Из-за аллокации памяти лучше не использовать каждый кадр.
        /// </summary>
        public static EntityBatch Get(params ComponentType[] componentTypes)
        {
            if (componentTypes == null || componentTypes.Length == 0)
                throw new ArgumentException("Укажите хотя бы один компонент для поиска.");

            var queryDesc = new EntityQueryDesc { All = componentTypes };
            var query = Manager.CreateEntityQuery(queryDesc);
            var entityArray = query.ToEntityArray(Allocator.Temp);

            var entityList = new NativeList<Entity>(entityArray.Length, Allocator.Persistent);
            entityList.AddRange(entityArray);

            entityArray.Dispose();
            query.Dispose();

            return new EntityBatch(entityList, Manager);
        }

        public static DotsCommand Get(this DotsCommand сommand, params ComponentType[] componentTypes) =>
            сommand.Do(batch => Get(componentTypes));

        /// <summary>
        /// Получает сущности по 1 компоненту-маркеру.
        /// ВНИМАНИЕ: Обязательно вызовите Dispose() у батча для избежания утечек памяти!
        /// Скорость: Средняя (Аллокация NativeList).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch Get<T1>()
            where T1 : struct, IComponentData
            => Get(ComponentType.ReadOnly<T1>());

        public static DotsCommand Get<T1>(this DotsCommand сommand)
            where T1 : struct, IComponentData => сommand.Do(batch => Get<T1>());

        /// <summary>
        /// Получает сущности по 2 компонентам-маркерам.
        /// ВНИМАНИЕ: Обязательно вызовите Dispose() у батча для избежания утечек памяти!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch Get<T1, T2>()
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            => Get(ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>());

        /// <summary>
        /// Получает сущности по 3 компонентам-маркерам.
        /// ВНИМАНИЕ: Обязательно вызовите Dispose() у батча для избежания утечек памяти!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch Get<T1, T2, T3>()
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            => Get(ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>(), ComponentType.ReadOnly<T3>());

        /// <summary>
        /// Получает сущности по 4 компонентам-маркерам.
        /// ВНИМАНИЕ: Обязательно вызовите Dispose() у батча для избежания утечек памяти!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch Get<T1, T2, T3, T4>()
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            => Get(ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>(), ComponentType.ReadOnly<T3>(), ComponentType.ReadOnly<T4>());

        /// <summary>
        /// Получает сущности по 5 компонентам-маркерам.
        /// ВНИМАНИЕ: Обязательно вызовите Dispose() у батча для избежания утечек памяти!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch Get<T1, T2, T3, T4, T5>()
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            where T5 : struct, IComponentData
            => Get(ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>(), ComponentType.ReadOnly<T3>(), ComponentType.ReadOnly<T4>(), ComponentType.ReadOnly<T5>());

        /// <summary>
        /// Получает сущности по 6 компонентам-маркерам.
        /// ВНИМАНИЕ: Обязательно вызовите Dispose() у батча для избежания утечек памяти!
        /// Скорость: Средняя (Аллокация NativeList).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch Get<T1, T2, T3, T4, T5, T6>()
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            where T5 : struct, IComponentData
            where T6 : struct, IComponentData
            => Get(ComponentType.ReadOnly<T1>(), ComponentType.ReadOnly<T2>(), ComponentType.ReadOnly<T3>(), ComponentType.ReadOnly<T4>(), ComponentType.ReadOnly<T5>(), ComponentType.ReadOnly<T6>());


    }
}
