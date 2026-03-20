using System.Runtime.CompilerServices;
using Unity.Entities;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Тип вывода логов для дебага моста DOTS.
        /// </summary>
        public enum BridgeLogType
        {
            Normal,
            Warning,
            Error
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PrintLog(BridgeLogType logType, string message)
        {
            switch (logType)
            {
                case BridgeLogType.Normal: UnityEngine.Debug.Log(message); break;
                case BridgeLogType.Warning: UnityEngine.Debug.LogWarning(message); break;
                case BridgeLogType.Error: UnityEngine.Debug.LogError(message); break;
            }
        }
        /// <summary>
        /// Выводит в консоль сообщение, если количество сущностей в батче больше указанного порога.
        /// Скорость: Мгновенно (O(1)).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity LogCount(this ListEntity batch, BridgeLogType logType = BridgeLogType.Normal, string customMessage = "")
        {
            PrintLog(logType, "Количество сущностей: " + batch.Count() + ", " + customMessage);
            return batch; // Возвращаем batch для поддержки цепочек вызовов (Fluent API)
        }   
        /// <summary>
        /// Выводит в консоль сообщение, если количество сущностей в батче больше указанного порога.
        /// Скорость: Мгновенно (O(1)).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity LogName(this ListEntity batch, BridgeLogType logType = BridgeLogType.Normal, string customMessage = "")
        {
            for (int i = 0; i < batch.Count; i++)
            {
                PrintLog(logType, "Имя у сущьности: " + batch.Entities[i].ToSingleEntity(batch.Word).GetComponent<BridgeIdentity>().Hash + ", " + customMessage);
            }
            return batch; // Возвращаем batch для поддержки цепочек вызовов (Fluent API)
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity LogIfContainsName(this ListEntity batch, string targetName, BridgeLogType logType = BridgeLogType.Normal)
        {
            int targetHash = GetHash(targetName);
            bool found = false;

            for (int i = 0; i < batch.Count; i++)
            {
                var single = batch.Entities[i].ToSingleEntity(batch.Word);
        
                // Проверяем, есть ли вообще компонент идентификации
                if (single.HasComponent<BridgeIdentity>())
                {
                    int entityHash = single.GetComponent<BridgeIdentity>().Hash;

                    if (entityHash == targetHash)
                    {
                        PrintLog(logType, $"[УСПЕХ] Сущность найдена! Имя: '{targetName}', Hash: {entityHash}, Index: {i}");
                        found = true;
                    }
                }
            }

            if (!found)
            {
                PrintLog(BridgeLogType.Warning, $"[ВНИМАНИЕ] Имя '{targetName}' не найдено в данном списке сущностей.");
            }

            return batch;
        }

        /// <summary>
        /// Выводит в консоль сообщение, если количество сущностей в батче больше указанного порога.
        /// Скорость: Мгновенно (O(1)).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity LogIfCountGreaterThan(this ListEntity batch, int threshold, BridgeLogType logType = BridgeLogType.Normal, string customMessage = "")
        {
            int count = batch.Count();
            if (count > threshold)
            {
                string msg = string.IsNullOrEmpty(customMessage)
                    ? $"[DotsBridge] Количество сущностей ({count}) превышает порог ({threshold})."
                    : $"[DotsBridge] {customMessage} | Сущностей: {count} (Порог: {threshold})";

                PrintLog(logType, msg);
            }
            return batch; // Возвращаем batch для поддержки цепочек вызовов (Fluent API)
        }

        /// <summary>
        /// Выводит в консоль сообщение, если батч пуст или количество сущностей меньше либо равно нулю.
        /// Скорость: Мгновенно (O(1)).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity LogIfCountLessOrEqualZero(this ListEntity batch, BridgeLogType logType = BridgeLogType.Warning, string customMessage = "")
        {
            int count = batch.Count();
            if (count <= 0)
            {
                string msg = string.IsNullOrEmpty(customMessage)
                    ? $"[DotsBridge] Батч пуст (Количество сущностей: {count})."
                    : $"[DotsBridge] {customMessage} | Сущностей: {count}";

                PrintLog(logType, msg);
            }
            return batch;
        }

        /// <summary>
        /// Выводит в консоль сообщение, если количество сущностей с указанным компонентом больше порога.
        /// ВНИМАНИЕ: Вызывает Sync Point для безопасного чтения.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity LogIfComponentCountGreaterThan<T>(this ListEntity batch, int threshold, BridgeLogType logType = BridgeLogType.Normal, string customMessage = "") where T : unmanaged, IComponentData
        {
            if (batch.Entities.IsEmpty) return batch;

            // Sync Point для безопасности
            var query = batch.Manager.CreateEntityQuery(ComponentType.ReadOnly<T>());
            query.CompleteDependency();

            int count = 0;
            var entities = batch.Entities.AsArray();
            for (int i = 0; i < entities.Length; i++)
            {
                if (batch.Manager.HasComponent<T>(entities[i])) count++;
            }

            if (count > threshold)
            {
                string msg = string.IsNullOrEmpty(customMessage)
                    ? $"[DotsBridge] Количество компонентов {typeof(T).Name} ({count}) превышает порог ({threshold})."
                    : $"[DotsBridge] {customMessage} | Компонентов {typeof(T).Name}: {count} (Порог: {threshold})";

                PrintLog(logType, msg);
            }
            return batch;
        }

        /// <summary>
        /// Выводит в консоль сообщение, если количество сущностей с указанным компонентом меньше либо равно нулю.
        /// ВНИМАНИЕ: Вызывает Sync Point для безопасного чтения.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListEntity LogIfComponentCountLessOrEqualZero<T>(this ListEntity batch, BridgeLogType logType = BridgeLogType.Warning, string customMessage = "") where T : unmanaged, IComponentData
        {
            if (batch.Entities.IsEmpty)
            {
                PrintLog(logType, string.IsNullOrEmpty(customMessage) ? $"[DotsBridge] Батч пуст, компонентов {typeof(T).Name}: 0." : $"[DotsBridge] {customMessage} | Компонентов {typeof(T).Name}: 0");
                return batch;
            }

            // Sync Point для безопасности
            var query = batch.Manager.CreateEntityQuery(ComponentType.ReadOnly<T>());
            query.CompleteDependency();

            int count = 0;
            var entities = batch.Entities.AsArray();
            for (int i = 0; i < entities.Length; i++)
            {
                if (batch.Manager.HasComponent<T>(entities[i])) count++;
            }

            if (count <= 0)
            {
                string msg = string.IsNullOrEmpty(customMessage)
                    ? $"[DotsBridge] Компоненты {typeof(T).Name} отсутствуют в батче (Количество: {count})."
                    : $"[DotsBridge] {customMessage} | Компонентов {typeof(T).Name}: {count}";

                PrintLog(logType, msg);
            }
            return batch;
        }
    }
}