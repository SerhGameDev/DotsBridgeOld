using Unity.Entities;
using UnityEngine;

namespace SimOil.Authoring
{
    public class FluidPipeAuthoring : MonoBehaviour
    {
        [Header("Топология")]
        [Tooltip("Откуда течет (Узел А)")]
        public GameObject nodeA;
        [Tooltip("Куда течет (Узел Б)")]
        public GameObject nodeB;

        [Header("Параметры трубы")]
        [Tooltip("Площадь сечения (м2)")]
        public float crossSectionArea = 0.05f;

        public class Baker : Unity.Entities.Baker<FluidPipeAuthoring>
        {
            public override void Bake(FluidPipeAuthoring authoring)
            {
                if (authoring.nodeA == null || authoring.nodeB == null) return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new FluidLink
                {
                    // Baker сам конвертирует GameObject'ы в их Entity-эквиваленты
                    NodeA = GetEntity(authoring.nodeA, TransformUsageFlags.Dynamic),
                    NodeB = GetEntity(authoring.nodeB, TransformUsageFlags.Dynamic),
                    CrossSectionArea = authoring.crossSectionArea,
                    CurrentFlowRateMass = 0f
                });
            }
        }
    }
}