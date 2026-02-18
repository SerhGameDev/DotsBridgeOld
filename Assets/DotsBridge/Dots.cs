using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace DotsBridge
{
    public static partial class Dots
    {
        #region Core & Context

        public static World World => World.DefaultGameObjectInjectionWorld;
        public static EntityManager Manager => World.EntityManager;

        #endregion
        public static int GetHash(string id) => new FixedString32Bytes(id).GetHashCode();

        #region Spawning (Sync & Async)

        #endregion


        private static Entity _prefabManagerEntity = Entity.Null;

        /// <summary>
        /// Получает Entity-префаб по имени GameObject'а.
        /// </summary>
        /// <param name="prefabName">Имя префаба (как в инспекторе)</param>
        public static Entity GetPrefab(string prefabName)
        {
            var manager = Manager;

            if (_prefabManagerEntity == Entity.Null || !manager.Exists(_prefabManagerEntity))
            {
                var query = manager.CreateEntityQuery(typeof(PrefabManagerTag));
                if (query.IsEmptyIgnoreFilter)
                {
                    UnityEngine.Debug.LogError($"[Dots] GlobalPrefabsAuthoring не найден на сцене! Создайте пустой объект и повесьте скрипт.");
                    return Entity.Null;
                }
                _prefabManagerEntity = query.GetSingletonEntity();
            }

            var buffer = manager.GetBuffer<PrefabLink>(_prefabManagerEntity);
            int targetHash = GetHash(prefabName);

            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].IDHash == targetHash)
                {
                    return buffer[i].PrefabEntity;
                }
            }

            UnityEngine.Debug.LogError($"[Dots] Префаб с именем '{prefabName}' не найден в списке GlobalPrefabsAuthoring!");
            return Entity.Null;
        }
    }
}
