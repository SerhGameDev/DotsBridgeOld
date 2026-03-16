using SimVent.Components;
using Unity.Entities;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SimVent.Authoring
{
    public class DpsSensorAuthoring : MonoBehaviour
    {
        [Title("Настройки реле давления (DPS)")]
        [Required, PropertyTooltip("Воздуховод, на котором физически стоит датчик")]
        public AirDuctAuthoring TargetDuct;

        [Title("Параметры срабатывания")]
        [SuffixLabel("м³/ч")]
        [PropertyTooltip("При каком расходе воздуха датчик должен замкнуться")]
        public float Setpoint = 1000f;

        [SuffixLabel("м³/ч")]
        [PropertyTooltip("Амплитуда колебаний воздуха при разгоне. Чем больше, тем дольше будет 'дребезг'.")]
        public float FlickerBand = 300f;

        class Baker : Baker<DpsSensorAuthoring>
        {
            public override void Bake(DpsSensorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new DpsSensorComponent
                {
                    TargetDuct = GetEntity(authoring.TargetDuct, TransformUsageFlags.None),
                    Setpoint = authoring.Setpoint,
                    FlickerBand = authoring.FlickerBand,
                    OutputSignal = false
                });
            }
        }
    }
}