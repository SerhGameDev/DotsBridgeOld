using Unity.Entities;
using UnityEngine;

namespace SimOil.Tests
{
    public class FlowTestAuthoring : MonoBehaviour
    {
        public class Baker : Unity.Entities.Baker<FlowTestAuthoring>
        {
            public override void Bake(FlowTestAuthoring authoring)
            {
                var linkEntity = GetEntity(TransformUsageFlags.Dynamic); 
                
                // Создаем буфер для связывания дополнительных сущностей с главной (важно для призраков)
                var linkedGroup = AddBuffer<LinkedEntityGroup>(linkEntity);
                linkedGroup.Add(linkEntity);

                // --- УЗЕЛ А ---
                var nodeA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                linkedGroup.Add(nodeA); // Привязываем к трубе
                
                AddComponent(nodeA, new FluidMixture { 
                    TotalMass = 1000f, Pressure = 2.0f, Temperature = 50f,
                    FractionMazut = 0.5f, FractionWater = 0.5f 
                });
                // Добавляем сетевой компонент с нулями (система перезапишет их)
                AddComponent(nodeA, new NetSync_FluidNode());

                // --- УЗЕЛ Б ---
                var nodeB = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                linkedGroup.Add(nodeB); // Привязываем к трубе
                
                AddComponent(nodeB, new FluidMixture { 
                    TotalMass = 10f, Pressure = 0.1f, Temperature = 20f,
                    FractionLightNaphtha = 1.0f 
                });
                AddComponent(nodeB, new NetSync_FluidNode());

                // --- ТРУБА ---
                AddComponent(linkEntity, new FluidLink {
                    NodeA = nodeA, NodeB = nodeB, CrossSectionArea = 0.05f
                });
                AddComponent(linkEntity, new PumpData {
                    MaxPressureBoost = 5.0f, CurrentPower = 1.0f 
                });
            }
        }
    }
}