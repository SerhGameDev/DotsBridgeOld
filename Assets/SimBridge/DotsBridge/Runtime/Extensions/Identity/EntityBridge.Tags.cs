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
        public static void RegisterTagServer<T>(this BridgeWorld world,string tagName) where T : struct, IComponentData
            => world.TagRegistry[tagName] = ComponentType.ReadOnly<T>();

        public static ListEntity AddByTags(this ListEntity list, string[] tags) 
            => GetByTags(list.Word, tags);

        public static DotsCommand AddByTags(this DotsCommand command, string[] tags)
            => command.Do(list => AddByTags(list, tags));

        public static ListEntity GetByTags(this BridgeWorld word, string[] tags)
        {
            if (word == null || tags == null || tags.Length == 0) return default;

            var types = new ComponentType[tags.Length];
            for (int i = 0; i < tags.Length; i++)
            {
                if (word.TagRegistry.TryGetValue(tags[i], out var type))
                    types[i] = type;
                else
                    return default;
            }

            return GetEntitiesFromContainer(word, types);
        }
        public static DotsCommand GetByTags(this DotsCommand command, string[] tags)
            => command.Do(list => GetByTags(list.Word, tags));
    }
}
