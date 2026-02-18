using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Spawning
{
    public struct SpawnRequest : IComponentData
    {
        public Entity Prefab;

        public int CountRemaining;
        public int OriginalCount; 

        // ГДЕ
        public float3 Position;
        public quaternion Rotation;

        // РАЗМЕР
        public float Scale;
        public bool OverrideScale;

        // ИДЕНТИФИКАТОР
        public int ID; // Хеш (FixedString или int, как у вас сделано)

        // НАСТРОЙКИ ВРЕМЕНИ
        public int BatchSize;
        public float Interval;
        public float Timer;

        // ЦИКЛЫ И КОНТРОЛЬ
        public int Loops; // -1 = Бесконечно, 1 = Один раз (стандарт), >1 количество повторов
        public bool IsPaused; // <--- Флаг паузы
    }
}