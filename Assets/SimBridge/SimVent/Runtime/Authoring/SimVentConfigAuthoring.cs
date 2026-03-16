using SimVent.Components;
using Unity.Entities;
using UnityEngine;

namespace SimVent.Authoring
{
    public class SimVentConfigAuthoring : MonoBehaviour
    {
        [Header("Аэродинамика")]
        public float BaseDuctResistance = 0.5f;
        public float DuctFrictionPerMeter = 1.5f; // Сопротивление на 1 Volume узла

        [Header("Термодинамика")]
        public float AirHeatCapacityConstant = 2985f;
        public float DeadHeadOverheatRate = 50f;
        public float DeadHeadCooldownRate = 0.05f;
        public float PressureMultiplier = 10000f;

        class Baker : Baker<SimVentConfigAuthoring>
        {
            public override void Bake(SimVentConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SimVentConfigComponent
                {
                    BaseDuctResistance = authoring.BaseDuctResistance,
                    DuctFrictionPerMeter = authoring.DuctFrictionPerMeter,
                    AirHeatCapacityConstant = authoring.AirHeatCapacityConstant,
                    DeadHeadOverheatRate = authoring.DeadHeadOverheatRate,
                    DeadHeadCooldownRate = authoring.DeadHeadCooldownRate,
                    PressureMultiplier = authoring.PressureMultiplier
                });
            }
        }
    }
}