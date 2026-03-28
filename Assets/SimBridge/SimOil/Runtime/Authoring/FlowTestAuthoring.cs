using Unity.Entities;
using UnityEngine;

namespace SimOil.Tests
{
    public class FlowTestAuthoring : MonoBehaviour
    {
        // Ссылки на дочерние объекты в инспекторе
        public GameObject NodeAObject;
        public GameObject NodeBObject;

        public class Baker : Baker<FlowTestAuthoring>
        {
            public override void Bake(FlowTestAuthoring authoring)
            {
                if (authoring.NodeAObject == null || authoring.NodeBObject == null) return;

                var linkEntity = GetEntity(TransformUsageFlags.Dynamic); 
                
                // Получаем сущности из реальных GameObject'ов
                var nodeA = GetEntity(authoring.NodeAObject, TransformUsageFlags.Dynamic);
                var nodeB = GetEntity(authoring.NodeBObject, TransformUsageFlags.Dynamic);

                AddComponent(nodeA, new FluidMixture { 
                    TotalMass = 1000f, 
                    Pressure = 2.0f, 
                    Temperature = 50f,
                    FractionMazut = 0.5f,
                    FractionWater = 0.5f 
                });

                AddComponent(nodeB, new FluidMixture { 
                    TotalMass = 10f, 
                    Pressure = 0.1f, 
                    Temperature = 20f,
                    FractionLightNaphtha = 1.0f 
                });

                AddComponent(linkEntity, new FluidLink {
                    NodeA = nodeA,
                    NodeB = nodeB,
                    CrossSectionArea = 0.05f 
                });

                AddComponent(linkEntity, new PumpData {
                    MaxPressureBoost = 5.0f, 
                    CurrentPower = 1.0f 
                });
            }
        }
    }
}