using System;
using Unity.Entities;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        // <summary>
        /// Начинает процесс спавна префаба, используя клиентский реестр.
        /// </summary>
        /// <param name="namePrefab">Имя префаба в реестре.</param>
        /// <returns>Строитель (Builder) для настройки параметров спавна.</returns>
        public static SpawnerBuilder BeginSpawnForClient(string namePrefab)
        {
            return BeginSpawn(namePrefab, ClientRegistry);
        }

        /// <summary>
        /// Начинает процесс спавна префаба, используя серверный реестр.
        /// </summary>
        /// <param name="namePrefab">Имя префаба в реестре.</param>
        /// <returns>Строитель (Builder) для настройки параметров спавна.</returns>
        public static SpawnerBuilder BeginSpawnForServer(string namePrefab)
        {
            return BeginSpawn(namePrefab, ServerRegistry);
        }

        /// <summary>
        /// Внутренний метод инициализации спавна через конкретный реестр.
        /// </summary>
        private static SpawnerBuilder BeginSpawn(string prefabName, BridgeRegistry registry)
        {
            var targetRegistry = registry ?? GetActiveRegistry();

            if (targetRegistry == null)
            {
                Debug.LogError($"[DotsBridge] Невозможно начать спавн '{prefabName}'. Реестр не инициализирован.");
                return new SpawnerBuilder(null, Entity.Null);
            }

            Entity prefab = GetPrefab(prefabName, targetRegistry);
            return new SpawnerBuilder(targetRegistry, prefab);
        }


        /// <summary>
        /// Вариант 1: Спавн "пустышки" на определенный временной промежуток.
        /// Создает пустую сущность и добавляет таймер уничтожения.
        /// </summary>
        public static EntityBatch QuickSpawnEmpty(this EntityBatch batch, float duration)
        {
            batch.Manager.AddComponentData(batch.Create(), new DestroyTimer { Value = duration });
            return batch;
        }

        /// <summary>
        /// Вариант 2: Спавн "пустышки" ровно на один кадр.
        /// Полезно для триггеров или событий, которые должны исчезнуть немедленно.
        /// </summary>
        public static EntityBatch SpawnEmptyOneFrame(this EntityBatch batch)
        {
            batch.Manager.AddComponentData(batch.Create(), new DestroyTimer { Value = 0 });
            return batch;
        }
    }
}