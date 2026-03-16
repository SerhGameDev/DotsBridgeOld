using SimVent.Components;
using Unity.Entities;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SimVent.Authoring
{
    public class ElectricHeaterAuthoring : MonoBehaviour
    {
        [Title("Где стоит (Труба)?")]
        [Required] public AirDuctAuthoring TargetDuct;

        [Title("Защита от перегрева")]
        [SuffixLabel("°C")] public float OverheatThreshold = 70f;

        [Title("Характеристики ТЭНа")]
        [SuffixLabel("кВт")] public float MaxPowerKW = 15f;
        [SuffixLabel("сек")] public float HeatingTimeConstant = 20f;

        class Baker : Baker<ElectricHeaterAuthoring>
        {
            public override void Bake(ElectricHeaterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ElectricHeaterComponent
                {
                    TargetDuct = GetEntity(authoring.TargetDuct, TransformUsageFlags.None), // <-- ДОБАВЛЕНО
                    TargetPower = 0f,
                    CurrentHeatOutput = 0f,
                    IsOverheated = false,
                    OverheatThreshold = authoring.OverheatThreshold,
                    MaxPowerKW = authoring.MaxPowerKW,
                    HeatingTimeConstant = authoring.HeatingTimeConstant
                });
            }
        }
    }
}