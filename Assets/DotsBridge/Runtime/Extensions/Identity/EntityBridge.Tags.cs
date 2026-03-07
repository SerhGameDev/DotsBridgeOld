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
        public static void RegisterTagServer<T>(string tagName) where T : struct, IComponentData
            => ServerRegistry.TagRegistry[tagName] = ComponentType.ReadOnly<T>();
        /// <summary>
        /// Поиск по ТЕГАМ (Компонентам-маркерам)
        /// </summary>
        public static EntityBatch GetByTagServer(params string[] tags) => GetByTags(ServerRegistry, tags);

        /// <summary>
        /// Поиск по ТЕГАМ на клиенте
        /// </summary>
        public static EntityBatch GetByTagClient(params string[] tags) => GetByTags(ClientRegistry, tags);
        /// <summary>
        /// Получает сущности, содержащие ВСЕ указанные строковые теги.
        /// ВНИМАНИЕ: Обязательно вызовите Dispose() у батча для избежания утечек памяти!
        /// Скорость: Средняя (Поиск в словаре + аллокация NativeList).
        /// Лимит: Удобно для разовых событий и скриптинга. Избегать частого вызова в Update.
        /// </summary>
        public static EntityBatch GetForClient(params string[] tags) => GetByTags(ClientRegistry, tags);

        private static EntityBatch GetByTags(BridgeRegistry registry, string[] tags)
        {
            if (registry == null || tags == null || tags.Length == 0) return default;

            var types = new ComponentType[tags.Length];
            for (int i = 0; i < tags.Length; i++)
            {
                if (registry.TagRegistry.TryGetValue(tags[i], out var type))
                    types[i] = type;
                else
                    return default;
            }

            return Get(registry, types); 
        }
    }
}
