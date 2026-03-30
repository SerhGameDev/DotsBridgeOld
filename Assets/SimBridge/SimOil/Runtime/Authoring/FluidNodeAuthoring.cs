using Unity.Entities;
using UnityEngine;

namespace SimOil.Tests
{
    public class FluidNodeAuthoring : MonoBehaviour
    {
        [Header("Initial State")]
        public float InitialMass = 1000f;
        public float InitialPressure = 2.0f;
        public float InitialTemperature = 50f;

        public class Baker : Unity.Entities.Baker<FluidNodeAuthoring>
        {
            public override void Bake(FluidNodeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                // 1. Тяжелая физика (для сервера)
                AddComponent(entity, new FluidMixture 
                { 
                    TotalMass = authoring.InitialMass, 
                    Pressure = authoring.InitialPressure, 
                    Temperature = authoring.InitialTemperature,
                    // Заполним чем-нибудь, чтобы избежать деления на ноль при смешивании
                    FractionWater = 1.0f 
                });

                // 2. Легкая сетевая оболочка (для синхронизации)
                AddComponent(entity, new NetSync_FluidNode());
            }
        }
    }
}