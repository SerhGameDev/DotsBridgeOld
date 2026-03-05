using System.Runtime.CompilerServices;
using Unity.Entities;

namespace DotsBridge
{
    public static partial class EntityBridge
    {

        /// <summary>
        /// Проверяет, существует ли компонент у ПЕРВОЙ сущности в батче.
        /// Скорость: Мгновенно (O(1)). Не вызывает Sync Point, так как просто читает метаданные архетипа.
        /// Лимит: Отсутствует. Можно вызывать сколько угодно раз за кадр.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponent<T>(this EntityBatch batch) where T : unmanaged, IComponentData
        {
            if (batch.Entities.IsEmpty) return false;

            // Проверка наличия компонента не требует ожидания фоновых потоков,
            // так как структура архетипа меняется только на главном потоке.
            return batch.Manager.HasComponent<T>(batch.Entities[0]);
        }

        /// <summary>
        /// Проверяет, включен ли компонент (IEnableableComponent) у ПЕРВОЙ сущности в батче.
        /// Скорость: Молниеносно (O(1)), но вызывает Sync Point для безопасного чтения.
        /// Лимит: Использовать для проверок состояний (например, жив ли игрок, в стане ли он).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsComponentEnabled<T>(this EntityBatch batch)
            where T : unmanaged, IComponentData, IEnableableComponent
        {
            if (batch.Entities.IsEmpty) return false;

            // Sync Point: ждем завершения фоновых задач, так как какая-то джоба 
            // могла прямо сейчас асинхронно переключать этот компонент.
            var query = batch.Manager.CreateEntityQuery(ComponentType.ReadOnly<T>());
            query.CompleteDependency();

            return batch.Manager.IsComponentEnabled<T>(batch.Entities[0]);
        }
        /// <summary>
        /// Возвращает количество сущностей в текущем батче.
        /// Скорость: Мгновенно (O(1)). Не вызывает Sync Point, так как просто читает длину закэшированного массива.
        /// Лимит: Отсутствует. Идеально подходит для проверок в Update.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count(this EntityBatch batch)
        {
            if (batch.Entities.IsEmpty) return 0;

            return batch.Entities.Length;
        }

        /// <summary>
        /// Проверяет, пустой ли батч (нет ни одной сущности).
        /// Скорость: Мгновенно (O(1)). Не вызывает Sync Point.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this EntityBatch batch)
        {
            return batch.Entities.IsEmpty || batch.Entities.Length == 0;
        }
    }
}