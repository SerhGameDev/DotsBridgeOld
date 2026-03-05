using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Регистрирует строковый тег и привязывает его к пустому компоненту (IComponentData).
        /// ВНИМАНИЕ: Без регистрации строковый поиск по тегу будет возвращать пустой список!
        /// Скорость: Мгновенно.
        /// Лимит: Вызывать только один раз при старте игры/сцены (Bootstrapper).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterTag<T>(string tagName) where T : struct, IComponentData
        {
            TagRegistry[tagName] = ComponentType.ReadOnly<T>();
        }

        /// <summary>
        /// Получает сущности, содержащие ВСЕ указанные строковые теги.
        /// ВНИМАНИЕ: Обязательно вызовите Dispose() у батча для избежания утечек памяти!
        /// Скорость: Средняя (Поиск в словаре + аллокация NativeList).
        /// Лимит: Удобно для разовых событий и скриптинга. Избегать частого вызова в Update.
        /// </summary>
        public static EntityBatch GetByTags(params string[] tags)
        {
            if (tags == null || tags.Length == 0)
                throw new ArgumentException("Укажите хотя бы один тег для поиска.");

            var types = new ComponentType[tags.Length];
            for (int i = 0; i < tags.Length; i++)
            {
                if (TagRegistry.TryGetValue(tags[i], out var type))
                {
                    types[i] = type;
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Тег '{tags[i]}' не зарегистрирован в DotsBridge.");
                    return new EntityBatch(new NativeList<Entity>(Allocator.Persistent), Manager);
                }
            }

            return Get(types);
        }

    }
}
