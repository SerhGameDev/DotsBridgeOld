using SimVent.Components;
using Unity.Entities;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SimVent.Authoring
{
    // --- АВТОРИНГ ДАТЧИКА ТЕМПЕРАТУРЫ ---
    public class TemperatureSensorAuthoring : MonoBehaviour
    {
        [Title("Настройки датчика температуры")]
        [Required, PropertyTooltip("Труба (Air Duct), в которой установлен датчик")]
        public AirDuctAuthoring TargetDuct; // <-- ИСПРАВЛЕНО ЗДЕСЬ

        [SuffixLabel("сек"), PropertyTooltip("Инерция измерительного элемента (защитной гильзы)")]
        public float SensorTimeConstant = 15f;

        class Baker : Baker<TemperatureSensorAuthoring>
        {
            public override void Bake(TemperatureSensorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new TemperatureSensorComponent
                {
                    TargetDuct = GetEntity(authoring.TargetDuct, TransformUsageFlags.None),
                    MeasuredTemperature = 20f, // Стартовое значение для красоты
                    SensorTimeConstant = authoring.SensorTimeConstant
                });
            }
        }
    }
}