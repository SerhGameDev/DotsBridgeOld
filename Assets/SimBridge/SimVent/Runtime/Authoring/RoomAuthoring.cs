using SimVent.Components;
using Unity.Entities;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SimVent.Authoring
{
    public class RoomAuthoring : MonoBehaviour
    {
        [Title("Связи помещения")]
        [Required, PropertyTooltip("Откуда поступает свежий воздух")]
        public AirDuctAuthoring SupplyDuct;

        [Title("Параметры помещения")]
        [SuffixLabel("м³")]
        public float Volume = 150f; // Например, комната 50 кв.м. с потолками 3м

        [PropertyTooltip("Насколько быстро комната остывает. 0.01 - хорошая изоляция, 0.1 - дырявый сарай")]
        public float HeatLossFactor = 0.02f;

        [Title("Начальные условия")]
        [SuffixLabel("°C")]
        public float StartTemperature = 5f; // Комната успела остыть

        [SuffixLabel("°C"), PropertyTooltip("Температура за бортом")]
        public float StreetTemperature = -15f;

        class Baker : Baker<RoomAuthoring>
        {
            public override void Bake(RoomAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new RoomComponent
                {
                    SupplyDuct = GetEntity(authoring.SupplyDuct, TransformUsageFlags.None),
                    CurrentTemperature = authoring.StartTemperature,
                    Volume = authoring.Volume,
                    HeatLossFactor = authoring.HeatLossFactor,
                    TargetStreetTemp = authoring.StreetTemperature
                });
            }
        }
    }
}