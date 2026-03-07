using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Получает или создает кэшированный контейнер сущностей по строковому ID.
        /// ВНИМАНИЕ: Метод скрыт, чтобы защитить словарь от внешних модификаций.
        /// Скорость: Высокая ($O(1)$ по словарю), но при первом вызове выполняет тяжелый поиск.
        /// Лимит: Идеально для уникальных/редких объектов на сцене (Игрок, Менеджер).
        /// </summary>
        private static EntityBatch GetById(BridgeRegistry registry, string idString)
        {
            if (registry == null) return default;
            var hash = GetHash(idString);

            if (!registry.Groups.TryGetValue(hash, out var group))
            {
                group = new EntityGroup(hash, registry.Manager);
                // Первичное заполнение (используем нашу реактивную логику или Find)
                // ... (здесь вызывается поиск в мире)
                registry.Groups.Add(hash, group);
            }
            return group.GetBatch();
        }

        /// <summary>
        /// Получает или создает кэшированный контейнер сущностей по строковому ID.
        /// ВНИМАНИЕ: Метод скрыт, чтобы защитить словарь от внешних модификаций.
        /// Скорость: Высокая ($O(1)$ по словарю), но при первом вызове выполняет тяжелый поиск.
        /// Лимит: Идеально для уникальных/редких объектов на сцене (Игрок, Менеджер).
        /// </summary>
        public static EntityBatch GetForServer(string id) => GetById(ServerRegistry, id);

        /// <summary>
        /// Получает или создает кэшированный контейнер сущностей по строковому ID.
        /// ВНИМАНИЕ: Метод скрыт, чтобы защитить словарь от внешних модификаций.
        /// Скорость: Высокая ($O(1)$ по словарю), но при первом вызове выполняет тяжелый поиск.
        /// Лимит: Идеально для уникальных/редких объектов на сцене (Игрок, Менеджер).
        /// </summary>
        public static EntityBatch GetClient(string id) => GetById(ClientRegistry, id);

        public static DotsCommand GetClient(this DotsCommand cmd, string id)
            => cmd.SetTargetResolver(() => GetClient(id), false);

        public static DotsCommand GetServer(this DotsCommand cmd, string id)
            => cmd.SetTargetResolver(() => GetForServer(id), false);
    }
}
