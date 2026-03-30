using Unity.Entities;
using UnityEngine;
using SimOil.Equipment;

namespace SimOil.Authoring
{
    public class FractionFilterAuthoring : MonoBehaviour
    {
        public FractionType allowedFraction = FractionType.Kerosene;
        public float minBoilingTemperature = 180f;

        public class Baker : Baker<FractionFilterAuthoring>
        {
            public override void Bake(FractionFilterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FractionFilter
                {
                    AllowedFraction = authoring.allowedFraction,
                    MinBoilingTemperature = authoring.minBoilingTemperature
                });
            }
        }
    }
}