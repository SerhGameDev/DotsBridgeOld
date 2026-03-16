using SimVent.Components;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

public class AirNodeAuthoring : MonoBehaviour
{
    [Tooltip("Отметьте, если это улица (бесконечный источник воздуха)")]
    public bool IsInfinite;

    [Title("Параметры")]
    public float StartTemperature = 20f;

    [HideIf("IsInfiniteStreet")]
    public float Volume = 100f;

    [HideIf("IsInfiniteStreet")]
    public float HeatLossFactor = 0.01f;

    // НОВЫЙ ПАРАМЕТР
    [HideIf("IsInfiniteStreet")]
    [PropertyTooltip("0 - полная герметичность, 0.5 - дырявая комната")]
    public float LeakFactor = 0.1f;

    class Baker : Baker<AirNodeAuthoring>
    {
        public override void Bake(AirNodeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new AirNodeComponent
            {
                IsInfinite = authoring.IsInfinite,
                Temperature = authoring.StartTemperature,
                Volume = authoring.IsInfinite ? 999999f : authoring.Volume,
                HeatLossFactor = authoring.HeatLossFactor,
                TargetStreetTemp = 20f,
                Pressure = 0f,
                LeakFactor = authoring.LeakFactor // <-- ПЕРЕДАЕМ В DOTS
            });
        }
    }
}