using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge
{
    // 1. Тег-маркер для всех объектов, которые должны попасть в файл сохранения

    public static partial class EntityBridge
    {
        /// <summary>
        /// Помечает батч сущностей как "сохраняемые" (добавляет тег)
        /// </summary>
        public static ListEntity MakeSaveable(this ListEntity batch)
        {
            return batch.AddComponent<SaveableTag>();
        }

        /// <summary>
        /// Помечает одиночную сущность как "сохраняемую"
        /// </summary>
        public static SingleEntity MakeSaveable(this SingleEntity entity)
        {
            return entity.AddComponent<SaveableTag>();
        }

        /// <summary>
        /// Синхронно собирает все сохраняемые объекты и передает их в коллбек.
        /// Вызывается напрямую из MonoBehaviour.
        /// </summary>
        public static void SaveGame(this BridgeWorld bridge, Action<ListEntity> onExtractLogic)
        {
            // Используем ваш метод FindWithComponent, который вернет батч с Temp аллокатором
            using var saveBatch = bridge.FindWithComponent<SaveableTag>();

            if (saveBatch.Count == 0)
            {
                Debug.LogWarning("[DotsBridge.SaveLoad] Нет сущностей с SaveableTag для сохранения.");
                return;
            }

            // Передаем батч пользователю
            onExtractLogic?.Invoke(saveBatch);
        }

        /// <summary>
        /// Очищает текущие сохраняемые объекты и вызывает коллбек для их пересоздания из файла.
        /// </summary>
        public static void LoadGame(this BridgeWorld bridge, Action<BridgeWorld> onRebuildLogic)
        {
            // Находим и уничтожаем старые сущности перед загрузкой, чтобы избежать дублирования
            using var oldSaveables = bridge.FindWithComponent<SaveableTag>();
            oldSaveables.ClearAndDestroy();

            // Вызываем пользовательскую логику для чтения файла и спавна префабов
            onRebuildLogic?.Invoke(bridge);
        }

        /// <summary>
        /// Утилита: Быстро извлекает все структуры компонента T из батча в обычный C# массив.
        /// Массив легко скармливается стандартному JsonUtility или Newtonsoft.Json.
        /// </summary>
        public static T[] ExtractComponentData<T>(this ListEntity batch) where T : unmanaged, IComponentData
        {
            var array = new T[batch.Count];
            for (int i = 0; i < batch.Count; i++)
            {
                if (batch.Manager.HasComponent<T>(batch.Entities[i]))
                {
                    array[i] = batch.Manager.GetComponentData<T>(batch.Entities[i]);
                }
            }
            return array;
        }

        /// <summary>
        /// Утилита: Применяет загруженный массив структур к сущностям в батче.
        /// </summary>
        public static ListEntity ApplyComponentData<T>(this ListEntity batch, T[] savedData) where T : unmanaged, IComponentData
        {
            if (savedData == null) return batch;

            int length = math.min(batch.Count, savedData.Length);
            for (int i = 0; i < length; i++)
            {
                if (batch.Manager.HasComponent<T>(batch.Entities[i]))
                {
                    batch.Manager.SetComponentData(batch.Entities[i], savedData[i]);
                }
            }
            return batch;
        }
    }
}