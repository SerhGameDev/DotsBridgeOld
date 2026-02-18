using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    /// <summary>
    /// Утилитарный класс для управления компонентами на сущностях.
    /// Обертка над EntityManager, упрощающая массовое добавление и удаление данных.
    /// </summary>
    public class ComponentUtils
    {
        private EntityManager _entityManager;

        /// <summary>
        /// Инициализирует контроллер, используя EntityManager из мира по умолчанию.
        /// </summary>
        /// <remarks>
        /// Внимание: Требует, чтобы World.DefaultGameObjectInjectionWorld уже был создан.
        /// </remarks>
        public ComponentUtils()
        {
            // Здесь стоит добавить проверку на null, аналогично предыдущему примеру,
            // чтобы избежать ошибок, если мир еще не инициализирован.
            if (World.DefaultGameObjectInjectionWorld != null)
            {
                _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            }
        }

        /// <summary>
        /// Добавляет компонент с данными к указанной сущности.
        /// </summary>
        /// <typeparam name="T">Тип компонента (должен быть unmanaged IComponentData).</typeparam>
        /// <param name="entity">Целевая сущность.</param>
        /// <param name="component">Данные компонента, которые нужно установить.</param>
        public void Add<T>(Entity entity, T component) where T : unmanaged, IComponentData
        {
            // Проверяем, жива ли сущность, прежде чем пытаться добавить компонент.
            if (_entityManager.Exists(entity))
            {
                // AddComponentData добавляет тип компонента и сразу устанавливает значение.
                // Примечание: Если компонент уже существует на сущности, этот метод может вызвать исключение.
                _entityManager.AddComponentData(entity, component);
            }
        }

        /// <summary>
        /// Массово добавляет компонент и устанавливает одно и то же значение для списка сущностей.
        /// </summary>
        /// <remarks>
        /// Операция оптимизирована: структурное изменение (добавление типа) происходит пакетно (Batch),
        /// что значительно быстрее, чем добавлять по одной сущности.
        /// </remarks>
        /// <typeparam name="T">Тип компонента.</typeparam>
        /// <param name="entities">Список сущностей (NativeList).</param>
        /// <param name="component">Значение, которое будет присвоено всем сущностям.</param>
        public void Add<T>(NativeList<Entity> entities, T component) where T : unmanaged, IComponentData
        {
            if (entities.Length == 0) return;

            // Преобразуем список в массив для использования в пакетных операциях API.
            NativeArray<Entity> entityArray = entities.AsArray();

            // 1. Структурное изменение: Добавляем тип компонента сразу всем сущностям.
            _entityManager.AddComponent<T>(entityArray);

            // 2. Установка данных: Проходим по массиву и проставляем значения.
            // Это самая затратная часть, но необходимая, если данные отличаются от дефолтных (default(T)).
            for (int i = 0; i < entityArray.Length; i++)
            {
                _entityManager.SetComponentData(entityArray[i], component);
            }
        }

        /// <summary>
        /// Массово удаляет компонент указанного типа у списка сущностей.
        /// </summary>
        /// <typeparam name="T">Тип удаляемого компонента.</typeparam>
        /// <param name="entities">Список сущностей.</param>
        public void Remove<T>(NativeList<Entity> entities) where T : unmanaged, IComponentData
        {
            if (entities.Length == 0) return;

            // Пакетное удаление компонента. Очень быстрая операция.
            _entityManager.RemoveComponent<T>(entities.AsArray());
        }
    }
}
