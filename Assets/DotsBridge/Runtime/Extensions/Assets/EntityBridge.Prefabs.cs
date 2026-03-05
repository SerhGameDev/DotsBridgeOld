using Unity.Entities;
using UnityEngine;


namespace DotsBridge
{
    public static partial class EntityBridge
    {
        private static DynamicBuffer<PrefabRegistryElement> _prefabBuffer;
        private static bool _isPrefabBufferCached = false;

        /// <summary>
        /// Возвращает Entity-префаб из глобального контейнера по его строковому имени.
        /// </summary>
        public static Entity GetPrefab(string name)
        {
            if (!_isPrefabBufferCached)
            {
                var query = Manager.CreateEntityQuery(typeof(PrefabRegistryElement));

                if (query.IsEmpty)
                {
                    UnityEngine.Debug.LogError("[DotsBridge] PrefabContainer не найден на сцене! Убедитесь, что объект с PrefabContainerAuthoring находится в SubScene.");
                    return Entity.Null;
                }

                _prefabBuffer = query.GetSingletonBuffer<PrefabRegistryElement>(true);
                _isPrefabBufferCached = true;
            }

            // 2. Ищем префаб по хэшу
            int hash = GetHash(name);
            foreach (var element in _prefabBuffer)
            {
                if (element.NameHash == hash)
                {
                    return element.PrefabEntity;
                }
            }

            UnityEngine.Debug.LogError($"[DotsBridge] Префаб с именем '{name}' не найден в контейнере!");
            return Entity.Null;
        }

        /// <summary>
        /// Сбрасывает кэш префабов (вызывать при смене сцены или перезапуске).
        /// </summary>
        public static void ClearPrefabCache()
        {
            _isPrefabBufferCached = false;
        }
    }
}