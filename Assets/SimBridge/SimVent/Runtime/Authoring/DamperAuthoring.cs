using SimVent.Components;
using Unity.Entities;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SimVent.Authoring
{
    public class DamperAuthoring : MonoBehaviour
    {
        [Title("Где стоит (Труба)?")]
        [Required] public AirDuctAuthoring TargetDuct; // <-- ДОБАВЛЕНО

        [Title("Характеристики привода заслонки")]
        [SuffixLabel("сек")] public float TransitTime = 30f;

        class Baker : Baker<DamperAuthoring>
        {
            public override void Bake(DamperAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new DamperComponent
                {
                    TargetDuct = GetEntity(authoring.TargetDuct, TransformUsageFlags.None), // <-- ДОБАВЛЕНО
                    TargetOpening = 0f,
                    CurrentOpening = 0f,
                    TransitTime = authoring.TransitTime
                });
            }
        }
    }
}