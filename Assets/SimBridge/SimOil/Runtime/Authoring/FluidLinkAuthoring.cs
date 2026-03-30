using Unity.Entities;
using UnityEngine;
using SimOil;

namespace SimOil.Tests
{
    public class FluidLinkAuthoring : MonoBehaviour
    {
        [Header("Connections")]
        public GameObject SourceNodeA;
        public GameObject TargetNodeB;

        [Header("Pipe Properties")]
        public float CrossSectionArea = 0.05f;
        public float PumpMaxBoost = 5.0f;
        public float PumpCurrentPower = 1.0f; // 1.0 = включен, 0.0 = выключен

        public class Baker : Unity.Entities.Baker<FluidLinkAuthoring>
        {
            public override void Bake(FluidLinkAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Защита от пустых ссылок в инспекторе
                if (authoring.SourceNodeA != null && authoring.TargetNodeB != null)
                {
                    // GetEntity автоматически свяжет GameObject-ы с их ECS сущностями
                    AddComponent(entity, new FluidLink {
                        NodeA = GetEntity(authoring.SourceNodeA, TransformUsageFlags.Dynamic),
                        NodeB = GetEntity(authoring.TargetNodeB, TransformUsageFlags.Dynamic),
                        CrossSectionArea = authoring.CrossSectionArea
                    });
                }

                AddComponent(entity, new PumpData {
                    MaxPressureBoost = authoring.PumpMaxBoost,
                    CurrentPower = authoring.PumpCurrentPower 
                });
            }
        }
    }
}