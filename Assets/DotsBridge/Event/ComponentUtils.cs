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
            if (_entityManager.Exists(entity))
            {
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

            NativeArray<Entity> entityArray = entities.AsArray();

            _entityManager.AddComponent<T>(entityArray);

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

            _entityManager.RemoveComponent<T>(entities.AsArray());
        }
    }
}
