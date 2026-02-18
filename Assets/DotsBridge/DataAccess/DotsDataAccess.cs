using Unity.Entities;

namespace DotsBridge
{
    public static partial class Dots
    {

        /// <summary>
        /// Получает значение компонента у сущности.
        /// </summary>
        public static T Get<T>(Entity entity) where T : unmanaged, IComponentData
        {

            if (!Manager.Exists(entity))
                return default;

            return Manager.GetComponentData<T>(entity);
        }

        /// <summary>
        /// Устанавливает значение компонента. Если компонента нет — добавляет его.
        /// </summary>
        public static void Set<T>(Entity entity, T data) where T : unmanaged, IComponentData
        {
            if (Manager.HasComponent<T>(entity))
            {
                Manager.SetComponentData(entity, data);
            }
            else
            {
                Manager.AddComponentData(entity, data);
            }
        }

        /// <summary>
        /// Проверяет наличие компонента.
        /// </summary>
        public static bool Has<T>(Entity entity) where T : unmanaged, IComponentData
        {
            return Manager.HasComponent<T>(entity);
        }

        /// <summary>
        /// Безопасно пытается получить компонент. Возвращает true, если получилось.
        /// </summary>
        public static bool TryGet<T>(Entity entity, out T data) where T : unmanaged, IComponentData
        {
            if (Manager.HasComponent<T>(entity))
            {
                data = Manager.GetComponentData<T>(entity);
                return true;
            }
            data = default;
            return false;
        }


        /// <summary>
        /// Получает данные синглтона. Если синглтона нет, возвращает default.
        /// </summary>
        public static T Get<T>() where T : unmanaged, IComponentData
        {
            var query = Manager.CreateEntityQuery(typeof(T));
            if (query.IsEmptyIgnoreFilter)
            {
                UnityEngine.Debug.LogWarning($"[Dots.Data] Singleton of type {typeof(T).Name} not found!");
                return default;
            }
            return query.GetSingleton<T>();
        }

        /// <summary>
        /// Устанавливает данные синглтона. 
        /// Если синглтона еще не существует — создает новую сущность с этим компонентом.
        /// </summary>
        public static void Set<T>(T data) where T : unmanaged, IComponentData
        {
            var query = Manager.CreateEntityQuery(typeof(T));

            if (query.IsEmptyIgnoreFilter)
            {
                // Синглтона нет -> Создаем его
                var entity = Manager.CreateEntity(typeof(T));
                Manager.SetComponentData(entity, data);
            }
            else
            {
                // Синглтон есть -> Обновляем
                query.SetSingleton(data);
            }
        }

        /// <summary>
        /// Проверяет, существует ли синглтон такого типа.
        /// </summary>
        public static bool Has<T>() where T : unmanaged, IComponentData
        {
            var query = Manager.CreateEntityQuery(typeof(T));
            return !query.IsEmptyIgnoreFilter;
        }

        /// <summary>
        /// Уничтожает синглтон (сущность, содержащую этот компонент).
        /// </summary>
        public static void RemoveSingleton<T>() where T : unmanaged, IComponentData
        {
            var query = Manager.CreateEntityQuery(typeof(T));
            if (!query.IsEmptyIgnoreFilter)
            {
                Manager.DestroyEntity(query);
            }
        }

        /// <summary>
        /// Получает буфер (массив данных) с сущности.
        /// </summary>
        public static DynamicBuffer<T> GetBuffer<T>(Entity entity) where T : unmanaged, IBufferElementData
        {
            return Manager.GetBuffer<T>(entity);
        }

    }
}
