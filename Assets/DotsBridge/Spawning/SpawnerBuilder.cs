using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace DotsBridge.Spawning
{
    public struct SpawnerBuilder
    {
        private EntityManager _manager;

        // Конфиг
        private Entity _prefab;
        private int _count;
        private float3 _position;
        private quaternion _rotation;
        private float _scale;
        private bool _overrideScale;
        private int _id;

        // Тайминг
        private int _batchSize;
        private float _interval;

        // Циклы
        private int _loops;
        private bool _isPaused;

        public SpawnerBuilder(EntityManager manager)
        {
            _manager = manager;

            _prefab = Entity.Null;
            _count = 1;
            _position = float3.zero;
            _rotation = quaternion.identity;
            _scale = 1f;
            _overrideScale = false;
            _id = 0;

            _batchSize = int.MaxValue;
            _interval = 0f;

            _loops = 1; // По умолчанию 1 проход
            _isPaused = false;
        }

        // ... SetPrefab, SetCount, SetPosition, SetRotation, SetScale, SetID (без изменений) ...

        public SpawnerBuilder SetPrefab(Entity prefab) { _prefab = prefab; return this; }
        public SpawnerBuilder SetCount(int count) { _count = count; return this; }
        public SpawnerBuilder SetPosition(Vector3 position) { _position = position; return this; }
        public SpawnerBuilder SetRotation(Quaternion rotation) { _rotation = rotation; return this; }
        public SpawnerBuilder SetID(string id) { _id = Dots.GetHash(id); return this; } // Предполагаем, что Dots.GetHash у вас есть

        public SpawnerBuilder SetScale(float scale)
        {
            _scale = scale;
            _overrideScale = true;
            return this;
        }

        // --- НОВЫЕ МЕТОДЫ ---

        /// <summary>
        /// Спаунить порциями через интервал.
        /// </summary>
        public SpawnerBuilder SetThrottle(int batchSize, float interval)
        {
            _batchSize = batchSize;
            _interval = interval;
            return this;
        }

        /// <summary>
        /// Растянуть спаун всего количества на заданное время.
        /// Система сама вычислит интервал.
        /// </summary>
        /// <param name="duration">Общее время спауна (сек)</param>
        /// <param name="batchSize">По сколько штук за раз (по умолчанию 1)</param>
        public SpawnerBuilder SetDuration(float duration, int batchSize = 1)
        {
            _batchSize = math.max(1, batchSize);

            // Сколько всего будет "тиков" (итераций спауна)
            // Например: 100 юнитов по 10 за раз = 10 тиков.
            int totalTicks = (int)math.ceil((float)_count / _batchSize);

            if (totalTicks > 0)
                _interval = duration / totalTicks;
            else
                _interval = 0;

            return this;
        }

        /// <summary>
        /// Настройка циклов.
        /// </summary>
        /// <param name="loops">1 = один раз, -1 = бесконечно</param>
        public SpawnerBuilder SetLoops(int loops)
        {
            _loops = loops;
            return this;
        }

        public SpawnerBuilder SetPaused(bool isPaused)
        {
            _isPaused = isPaused;
            return this;
        }

        public void Build()
        {
            Entity spawnerEntity = _manager.CreateEntity();

            _manager.AddComponentData(spawnerEntity, new SpawnRequest
            {
                Prefab = _prefab,
                CountRemaining = _count,
                OriginalCount = _count, // Запоминаем для цикла
                Position = _position,
                Rotation = _rotation,
                Scale = _scale,
                OverrideScale = _overrideScale,
                ID = _id,
                BatchSize = _batchSize,
                Interval = _interval,
                Timer = 0,
                Loops = _loops,
                IsPaused = _isPaused
            });
        }
    }
}