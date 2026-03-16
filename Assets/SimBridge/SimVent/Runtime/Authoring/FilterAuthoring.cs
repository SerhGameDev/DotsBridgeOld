using Unity.Entities;
using UnityEngine;
using SimVent.Components;

namespace SimVent.Authoring
{
    public class FilterAuthoring : MonoBehaviour
    {
        public AirDuctAuthoring TargetDuct;
        [Tooltip("Сопротивление чистого фильтра (0.1 - 0.2)")]
        public float NominalResistance = 0.1f;
        [Range(0, 1)]
        public float StartingDirtiness = 0f;
        [Tooltip("Добавочное сопротивление при 100% загрязнении (1.0 - 5.0)")]
        public float MaxDirtinessResistance = 2.0f;

        class Baker : Baker<FilterAuthoring>
        {
            public override void Bake(FilterAuthoring authoring)
            {
                if (authoring.TargetDuct == null) return;

                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new FilterComponent
                {
                    TargetDuct = GetEntity(authoring.TargetDuct, TransformUsageFlags.None),
                    NominalResistance = authoring.NominalResistance,
                    Dirtiness = authoring.StartingDirtiness,
                    MaxDirtinessResistance = authoring.MaxDirtinessResistance
                });
            }
        }
    }
}