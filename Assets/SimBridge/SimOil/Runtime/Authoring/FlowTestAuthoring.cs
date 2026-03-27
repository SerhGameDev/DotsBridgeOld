using Unity.Entities;
using UnityEngine;

namespace SimOil.Tests
{
    public class FlowTestAuthoring : MonoBehaviour
    {
        public class Baker : Baker<FlowTestAuthoring>
        {
            public override void Bake(FlowTestAuthoring authoring)
            {
                // Основная сущность этого GameObject будет нашей Трубой (Link)
                var linkEntity = GetEntity(TransformUsageFlags.Dynamic); 

                // Создаем невидимую сущность для Резервуара А (Источник)
                var nodeA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                AddComponent(nodeA, new FluidMixture { 
                    TotalMass = 1000f,     // 1 тонна жидкости
                    Pressure = 2.0f,       // Давление 2 МПа
                    Temperature = 50f,
                    // Состав: 50% мазута, 50% воды
                    FractionMazut = 0.5f,
                    FractionWater = 0.5f 
                });

                // Создаем невидимую сущность для Резервуара Б (Приемник)
                var nodeB = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                AddComponent(nodeB, new FluidMixture { 
                    TotalMass = 10f,       // Почти пустой (10 кг)
                    Pressure = 0.1f,       // Низкое давление
                    Temperature = 20f,
                    // Состав: 100% легкий бензин (остатки на дне)
                    FractionLightNaphtha = 1.0f 
                });

                // Настраиваем трубу, соединяя Узел А и Узел Б
                AddComponent(linkEntity, new FluidLink {
                    NodeA = nodeA,
                    NodeB = nodeB,
                    CrossSectionArea = 0.05f // Сечение трубы
                });

                // Вешаем насос на трубу, включенный на полную мощность
                AddComponent(linkEntity, new PumpData {
                    MaxPressureBoost = 5.0f, // Насос добавляет 5 МПа напора
                    CurrentPower = 1.0f      // Включен на 100%
                });
            }
        }
    }
}