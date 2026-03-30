using Unity.Entities;
using UnityEngine;

namespace SimOil.Authoring
{
    public class PumpAuthoring : MonoBehaviour
    {
        public float maxPressureBoost = 5.0f;
        [Range(0f, 1f)] public float currentPower = 1.0f;

        public class Baker : Baker<PumpAuthoring>
        {
            public override void Bake(PumpAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PumpData
                {
                    MaxPressureBoost = authoring.maxPressureBoost,
                    CurrentPower = authoring.currentPower
                });
            }
        }
    }
}