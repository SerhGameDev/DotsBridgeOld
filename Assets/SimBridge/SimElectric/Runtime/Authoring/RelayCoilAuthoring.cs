using SimElectric;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace SimElectric
{
    public class RelayCoilAuthoring : MonoBehaviour
    {
        [Title("Coil Connections")]
        [Required] public ElectricalNodeAuthoring nodeA1;
        [Required] public ElectricalNodeAuthoring nodeA2;

        [Title("Specs")]
        public float nominalVoltage = 24f;
        [Tooltip("Напряжение, при котором якорь притягивается")]
        public float energizeThreshold = 18f;

        public class Baker : Baker<RelayCoilAuthoring>
        {
            public override void Bake(RelayCoilAuthoring authoring)
            {
                if (authoring.nodeA1 == null || authoring.nodeA2 == null) return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RelayCoil
                {
                    NodeA1 = GetEntity(authoring.nodeA1, TransformUsageFlags.Dynamic),
                    NodeA2 = GetEntity(authoring.nodeA2, TransformUsageFlags.Dynamic),
                    NominalVoltage = authoring.nominalVoltage,
                    EnergizeThreshold = authoring.energizeThreshold,
                    IsEnergized = false
                });
            }
        }
    }
}