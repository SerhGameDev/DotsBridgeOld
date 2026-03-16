using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace SimElectric
{
    [RequireComponent(typeof(WireAuthoring))]
    public class CircuitBreakerAuthoring : MonoBehaviour
    {
        [Title("Circuit Breaker Settings")]
        [Tooltip("Ток срабатывания (Амперы)")]
        public float ratedCurrent = 16f;

        [Tooltip("Начальное состояние (выбит ли при старте)")]
        public bool isTripped = false;

        public class Baker : Baker<CircuitBreakerAuthoring>
        {
            public override void Bake(CircuitBreakerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CircuitBreaker
                {
                    RatedCurrent = authoring.ratedCurrent,
                    IsTripped = authoring.isTripped
                });
            }
        }
    }
}