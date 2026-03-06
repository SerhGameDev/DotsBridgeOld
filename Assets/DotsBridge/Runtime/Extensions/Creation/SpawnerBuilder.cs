using DotsBridge.Spawning;
using System.Security.Cryptography;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DotsBridge
{
    public struct SpawnerBuilder
    {
        private EntityManager _manager;
        private Entity _prefab;
        private int _count;
        private float3 _position;
        private quaternion _rotation;
        private float _scale;
        private bool _overrideScale;
        private int _id;

        public SpawnerBuilder(Entity prefab)
        {
            _manager = EntityBridge.Manager;
            _prefab = prefab;
            _count = 1;
            _position = float3.zero;
            _rotation = quaternion.identity;
            _scale = 1f; 
            _overrideScale = false;
            _id = 0;
        }
        // --- НОВЫЙ КОНСТРУКТОР ДЛЯ МУЛЬТИПЛЕЕРА ---
        public SpawnerBuilder(BridgeState state, Entity prefab)
        {
            // Здесь магия: мы берем EntityManager не из глобальной статики,
            // а из того мира (серверного или клиентского), который нам передали!
            _manager = state.Manager;

            _prefab = prefab;
            _count = 1;
            _position = float3.zero;
            _rotation = quaternion.identity;
            _scale = 1f;
            _overrideScale = false;
            _id = 0;
        }

        public SpawnerBuilder SetCount(int count) { _count = count; return this; }
        public SpawnerBuilder SetPosition(Vector3 position) { _position = position; return this; }
        public SpawnerBuilder SetRotation(Quaternion rotation) { _rotation = rotation; return this; }
        public SpawnerBuilder SetScale(float scale) { _scale = scale; return this; }
        /// <summary>
        /// Создает команду спавна. Опциональный 'id' автоматически зарегистрирует сущности в реактивной системе.
        /// </summary>
        public DotsCommand Spawn(string commandName = "SpawnCommand", string id = null)
        {
            var command = new DotsCommand(commandName);

            var prefab = _prefab;
            var count = _count;
            var pos = _position;
            var rot = _rotation;
            var scale = _scale;

            // 1. Резолвер теперь ПРИНИМАЕТ BridgeState в момент вызова Execute!
            command.SetTargetResolver((state) =>
            {
                // 2. Достаем актуальный менеджер из прилетевшего стейта
                var manager = state.Manager;

                if (prefab == Entity.Null)
                {
                    UnityEngine.Debug.LogError("[DotsBridge] Попытка заспавнить Entity.Null!");
                    // ОШИБКА ИСПРАВЛЕНА: Передаем state вместо manager
                    return new EntityBatch(default, state);
                }

                NativeList<Entity> spawnedEntities = new NativeList<Entity>(count, Allocator.TempJob);

                using (var tempArray = new NativeArray<Entity>(count, Allocator.Temp))
                {
                    manager.Instantiate(prefab, tempArray);
                    spawnedEntities.AddRange(tempArray);
                }

                var transform = Unity.Transforms.LocalTransform.FromPositionRotationScale(pos, rot, scale);

                // Считаем хэш заранее, если ID был передан
                int idHash = string.IsNullOrEmpty(id) ? 0 : EntityBridge.GetHash(id);

                foreach (var entity in spawnedEntities)
                {
                    manager.SetComponentData(entity, transform);

                    // 3. ИНТЕГРАЦИЯ С РЕАКТИВНОЙ СИСТЕМОЙ: 
                    // Вешаем бейджик, чтобы BridgeIdentitySyncSystem сразу нашла новичка!
                    if (idHash != 0)
                    {
                        manager.AddComponentData(entity, new BridgeIdentity { Hash = idHash });
                    }
                }

                // ОШИБКА ИСПРАВЛЕНА: Передаем state вместо manager
                return new EntityBatch(spawnedEntities, state);

            }, true); // true = батч (spawnedEntities) будет очищен после выполнения команды

            return command;
        }

        /// <summary>
        /// Создает заявку на спаун (SpawnRequest). Сами сущности будут созданы 
        /// в начале следующего кадра (InitializationSystemGroup) на фоновых потоках.
        /// Скорость: Максимальная. Без Sync Point на инстанцирование.
        /// ВНИМАНИЕ: Метод ничего не возвращает (Fire-and-Forget).
        /// </summary>
        public void SpawnAsync()
        {
            if (_prefab == Entity.Null)
            {
                UnityEngine.Debug.LogError("[DotsBridge] Попытка заспавнить Entity.Null через SpawnAsync!");
                return;
            }

            // 1. Создаем пустую сущность-контейнер для нашего компонента-заявки
            // Это легкая операция для главного потока
            Entity requestEntity = _manager.CreateEntity(typeof(SpawnRequest));

            // 2. Формируем саму заявку на основе данных из билдера
            var request = new SpawnRequest
            {
                Prefab = _prefab,
                Count = _count,
                Position = _position,
                Rotation = _rotation,
                Scale = _scale,
                OverrideScale = _overrideScale,
                ID = _id
            };

            // 3. Вешаем компонент-заявку на созданную сущность
            _manager.SetComponentData(requestEntity, request);
        }
    }
}