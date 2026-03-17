using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace DotsBridge
{
    /// <summary>
    /// Удобная обертка (фасад) для работы с ОДНОЙ сущностью в главном потоке.
    /// Не требует IDisposable, так как не выделяет неуправляемую память.
    /// </summary>
    public readonly struct SingleEntity
    {
        public readonly Entity Entity;
        public readonly BridgeWorld Bridge;
        public EntityManager Manager => Bridge.Manager;

        public SingleEntity(Entity entity, BridgeWorld bridge)
        {
            Entity = entity;
            Bridge = bridge;
        }
        /// <summary>
        /// Конвертирует одиночную сущность в ListEntity (группу из одного элемента).
        /// ВНИМАНИЕ: Создает новый NativeList, поэтому результат требует вызова Dispose()!
        /// </summary>
        public ListEntity ToListEntity(Allocator allocator = Allocator.Temp)
        {
            // Выделяем память ровно под 1 элемент
            var list = new NativeList<Entity>(1, allocator);

            // Если сущность жива, добавляем её в список
            if (Manager.Exists(Entity))
            {
                list.Add(Entity);
            }

            // Возвращаем ListEntity с флагом владения (isOwner = true).
            // Это гарантирует, что при вызове ApplyAndDispose() или выходе из using
            // эта память будет корректно очищена.
            return new ListEntity(Bridge, list, isOwner: true);
        }
        /// <summary>
        /// Включает или выключает конкретный компонент на сущности.
        /// Компонент T должен реализовывать интерфейс IEnableableComponent.
        /// </summary>
        public SingleEntity SetEnabled<T>(bool enabled) where T : unmanaged, IEnableableComponent
        {
            if (!Manager.Exists(Entity)) return this;

            // Если компонента нет, Unity выбросит исключение. 
            // Поэтому сначала проверяем наличие, либо просто устанавливаем состояние.
            if (Manager.HasComponent<T>(Entity))
            {
                Manager.SetComponentEnabled<T>(Entity, enabled);
            }

            return this;
        }

        /// <summary>
        /// Проверяет, включен ли конкретный компонент.
        /// </summary>
        public bool IsEnabled<T>() where T : unmanaged, IEnableableComponent
        {
            if (!Manager.Exists(Entity) || !Manager.HasComponent<T>(Entity))
                return false;

            return Manager.IsComponentEnabled<T>(Entity);
        }
        /// <summary>
        /// Создает одну сущность из префаба и возвращает фасад.
        /// </summary>
        public static SingleEntity Instantiate(BridgeWorld bridge, Entity prefab)
        {
            if (prefab == Entity.Null) return default;

            var entity = bridge.Manager.Instantiate(prefab);
            return new SingleEntity(entity, bridge);
        }

        /// <summary>
        /// Создает пустую сущность (пустышку). Можно передать архетип.
        /// </summary>
        public static SingleEntity CreateEmpty(BridgeWorld bridge, EntityArchetype archetype = default)
        {
            var entity = bridge.Manager.CreateEntity(archetype);
            return new SingleEntity(entity, bridge);
        }
        /// <summary>
        /// Создает сущность-синглтон с указанным компонентом.
        /// </summary>
        public static SingleEntity CreateSingleton<T>(BridgeWorld bridge) where T : unmanaged, IComponentData
        {
            var entity = bridge.Manager.CreateEntity();
            bridge.Manager.AddComponent<T>(entity);
            return new SingleEntity(entity, bridge);
        }
        /// <summary>
         /// Создает сущность на основе архетипа для использования в качестве синглтона.
         /// </summary>
        public static SingleEntity CreateSingleton(BridgeWorld bridge, EntityArchetype archetype)
        {
            var entity = bridge.Manager.CreateEntity(archetype);
            return new SingleEntity(entity, bridge);
        }
        /// <summary>
        /// Создает пустышку в конкретной позиции.
        /// </summary>
        public static SingleEntity CreateEmpty(BridgeWorld bridge, float3 position, EntityArchetype archetype = default)
        {
            var entity = bridge.Manager.CreateEntity(archetype);

            // Если в архетипе нет LocalTransform, метод AddComponentData безопасно его добавит
            bridge.Manager.AddComponentData(entity, LocalTransform.FromPosition(position));

            return new SingleEntity(entity, bridge);
        }

        /// <summary>
        /// Паттерн Singleton: Находит существующую сущность с компонентом T или создает новую.
        /// </summary>
        public static SingleEntity GetOrCreateSingleton<T>(BridgeWorld bridge) where T : unmanaged, IComponentData
        {
            var query = bridge.Manager.CreateEntityQuery(typeof(T));

            if (query.HasSingleton<T>())
            {
                var existingEntity = query.GetSingletonEntity();
                return new SingleEntity(existingEntity, bridge);
            }

            var newEntity = bridge.Manager.CreateEntity(typeof(T));
            return new SingleEntity(newEntity, bridge);
        }

        // =========================================================
        // МЕТОДЫ ЭКЗЕМПЛЯРА (FLUENT API)
        // =========================================================

        public SingleEntity AddComponent<T>() where T : unmanaged, IComponentData
        {
            Manager.AddComponent<T>(Entity);
            return this;
        }

        public SingleEntity AddComponent<T>(T componentData) where T : unmanaged, IComponentData
        {
            Manager.AddComponentData(Entity, componentData);
            return this;
        }

        public SingleEntity SetComponent<T>(T componentData) where T : unmanaged, IComponentData
        {
            Manager.SetComponentData(Entity, componentData);
            return this;
        }

        public bool HasComponent<T>() where T : unmanaged, IComponentData
        {
            return Manager.HasComponent<T>(Entity);
        }

        public T GetComponent<T>() where T : unmanaged, IComponentData
        {
            return Manager.GetComponentData<T>(Entity);
        }

        public void Destroy()
        {
            if (!Manager.Exists(Entity)) return;

            // Логика очистки из вашего BridgeWorld
            if (Bridge.OnDestroyEvents.TryGetValue(Entity, out var onDestroyAction))
            {
                onDestroyAction?.Invoke(Entity);
                Bridge.OnDestroyEvents.Remove(Entity);
            }

            Manager.DestroyEntity(Entity);
        }
    }
}