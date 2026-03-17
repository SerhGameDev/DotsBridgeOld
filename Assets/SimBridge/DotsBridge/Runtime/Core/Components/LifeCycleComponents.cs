using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    // 1. Метка: Сущность должна быть уничтожена в конце кадра
    public struct DestroyTag : IComponentData { }

    // 2. DOTS-данные: Какой префаб заспавнить при смерти (партиклы/лут)
    public struct SpawnOnDeath : IComponentData
    {
        public Entity Prefab;
    }

    public struct DeathEvent : IComponentData, IEnableableComponent { }

    public struct DestroyTimer : IComponentData
    {
        public float Value;
    }
}
