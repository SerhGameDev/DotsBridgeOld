using System;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Внутренний метод инициализации спавна через конкретный реестр.
        /// </summary>
        private static SpawnerBuilder BeginSpawn(this BridgeWorld word, string prefabName)
        {
            if (word == null)
            {
                Debug.LogError($"[DotsBridge] Невозможно начать спавн '{prefabName}'. Реестр не инициализирован.");
                return new SpawnerBuilder(null, Entity.Null);
            }

            return new SpawnerBuilder(word, GetPrefab(word, prefabName));
        }


        /// <summary>
        /// Вариант 1: Спавн "пустышки" на определенный временной промежуток.
        /// Создает пустую сущность и добавляет таймер уничтожения.
        /// </summary>
        public static ListEntity QuickSpawnEmpty(this ListEntity list, float duration = 1, int count = 1)
        {
            if(count > 1)
            {
                list.CreateEmpty(count);
                list.AddComponent(new DestroyTimer { Value = duration });
            }
            else if (duration == 0)
            {
                SpawnEmptyOneFrame(list);
            }
            else
            {
                Debug.LogWarning("[QuickSpawnEmpty] Попытка спауна меньше нуля entity");
            }
            return list;
        }

        /// <summary>
        /// Вариант 1: Спавн "пустышки" на определенный временной промежуток.
        /// Создает пустую сущность и добавляет таймер уничтожения.
        /// </summary>
        public static ListEntity QuickSpawnEmpty(this BridgeWorld world, float duration = 1, int count = 1)
        {
            var templist = new ListEntity(world);
            if (count > 1)
            {
                templist.CreateEmpty(count);
                templist.AddComponent(new DestroyTimer { Value = duration });
            }
            else if (duration == 0)
            {
                SpawnEmptyOneFrame(templist);
            }
            else
            {
                Debug.LogWarning("[QuickSpawnEmpty] Попытка спауна меньше нуля entity");
            }
            return templist;
        }

        /// <summary>
        /// Вариант 2: Спавн "пустышки" ровно на один кадр.
        /// Полезно для триггеров или событий, которые должны исчезнуть немедленно.
        /// </summary>
        public static ListEntity SpawnEmptyOneFrame(this ListEntity list)
        {
            list.CreateEmpty();
            list.AddComponent(new DestroyTimer { Value = 0 });
            return list;
        }
        /// <summary>
        /// Вариант 2: Спавн "пустышки" ровно на один кадр.
        /// Полезно для триггеров или событий, которые должны исчезнуть немедленно.
        /// </summary>
        public static ListEntity SpawnEmptyOneFrame(this BridgeWorld world)
        {
            var list = new ListEntity(world);
            list.CreateEmpty();
            list.AddComponent(new DestroyTimer { Value = 0 });
            return list;
        }
    }
}