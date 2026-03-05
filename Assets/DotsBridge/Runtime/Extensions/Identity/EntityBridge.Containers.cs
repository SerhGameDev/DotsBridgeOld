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
        private static EntityContainer GetOrCreateContainer(string idString)
        {
            var id = GetHash(idString);

            if (!Containers.TryGetValue(id, out var container))
            {
                container = new EntityContainer(id, Manager);

                // Первичное заполнение
                var tempBatch = Find(idString, Allocator.Temp);
                container.Entities.AddRange(tempBatch.Entities.AsArray());
                tempBatch.Dispose();

                Containers.Add(id, container);
            }
            return container;
        }

        /// <summary>
        /// Получает батч сущностей по уникальному строковому ID.
        /// ВНИМАНИЕ: Не вызывайте Dispose() у полученного батча! Сущностями управляет глобальный контейнер.
        /// Скорость: Очень высокая (мгновенный доступ к кэшу).
        /// Лимит: Безопасно вызывать каждый кадр (в методах Update).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityBatch GetById(string id)
        {
            return GetOrCreateContainer(id).GetBatch();
        }
    }
}
