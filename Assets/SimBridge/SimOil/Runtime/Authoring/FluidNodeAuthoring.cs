using Unity.Entities;
using UnityEngine;

namespace SimOil.Authoring
{
    public class FluidNodeAuthoring : MonoBehaviour
    {
        [Header("Термодинамика")]
        public float totalMass = 1000f;
        public float pressure = 1.0f;
        public float temperature = 20.0f;

        [Header("Состав (Доли от 0 до 1)")]
        [Range(0f, 1f)] public float fractionGas = 0f;
        [Range(0f, 1f)] public float fractionLightNaphtha = 0f;
        [Range(0f, 1f)] public float fractionHeavyNaphtha = 0f;
        [Range(0f, 1f)] public float fractionKerosene = 0f;
        [Range(0f, 1f)] public float fractionLightDiesel = 0f;
        [Range(0f, 1f)] public float fractionHeavyDiesel = 0f;
        [Range(0f, 1f)] public float fractionMazut = 0f;
        [Range(0f, 1f)] public float fractionWater = 1.0f; // По умолчанию вода

        private void OnValidate()
        {
            // Удобная проверка в инспекторе, чтобы сумма была равна 1
            float sum = fractionGas + fractionLightNaphtha + fractionHeavyNaphtha + fractionKerosene + 
                        fractionLightDiesel + fractionHeavyDiesel + fractionMazut + fractionWater;
            
            if (Mathf.Abs(sum - 1.0f) > 0.01f && totalMass > 0)
            {
                Debug.LogWarning($"[SimOil] Сумма фракций на объекте {gameObject.name} равна {sum}. Она должна быть равна 1.0!", gameObject);
            }
        }

        public class Baker : Unity.Entities.Baker<FluidNodeAuthoring>
        {
            public override void Bake(FluidNodeAuthoring authoring)
            {
                // Используем Dynamic, чтобы узлы можно было двигать/видеть в 3D
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new FluidMixture
                {
                    TotalMass = authoring.totalMass,
                    Pressure = authoring.pressure,
                    Temperature = authoring.temperature,
                    FractionGas = authoring.fractionGas,
                    FractionLightNaphtha = authoring.fractionLightNaphtha,
                    FractionHeavyNaphtha = authoring.fractionHeavyNaphtha,
                    FractionKerosene = authoring.fractionKerosene,
                    FractionLightDiesel = authoring.fractionLightDiesel,
                    FractionHeavyDiesel = authoring.fractionHeavyDiesel,
                    FractionMazut = authoring.fractionMazut,
                    FractionWater = authoring.fractionWater
                });
            }
        }
    }
}