using Unity.Entities;
using UnityEngine;
using SimOil.Equipment;

namespace SimOil.Authoring
{
    public class HeaterAuthoring : MonoBehaviour
    {
        public float maxHeatPower = 10000f; // 10 МВт
        [Range(0f, 1f)] public float currentPower = 0f;
        [Range(0f, 1f)] public float heatLossFactor = 0.05f;

        public class Baker : Baker<HeaterAuthoring>
        {
            public override void Bake(HeaterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new HeaterData
                {
                    MaxHeatPower = authoring.maxHeatPower,
                    CurrentPower = authoring.currentPower,
                    HeatLossFactor = authoring.heatLossFactor
                });
            }
        }
    }
}