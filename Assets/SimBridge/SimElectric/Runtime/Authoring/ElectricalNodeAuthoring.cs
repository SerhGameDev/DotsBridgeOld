using SimElectric;
using Unity.Entities;
using UnityEngine;

namespace SimElectric
{
    public class ElectricalNodeAuthoring : MonoBehaviour
    {
        [Header("Node Settings")]
        public bool isPowerSource;
        public float sourceVoltage = 24f;
        public bool isGrounded; 
        [Header("Physics")]
        [Tooltip("Сопротивление (Ом). Для проводов/клемм = 0. Для ламп/катушек = 300-500")]
        public float resistance = 0f;

        // Baker класс преобразует данные из MonoBehaviour в ECS
        public class ElectricalNodeBaker : Baker<ElectricalNodeAuthoring>
        {
            public override void Bake(ElectricalNodeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new ElectricalNode
                {
                    CurrentVoltage = authoring.isGrounded ? 0f : (authoring.isPowerSource ? authoring.sourceVoltage : 0f),
                    IsPowerSource = authoring.isPowerSource,
                    SourceVoltage = authoring.sourceVoltage,
                    IsGrounded = authoring.isGrounded,
                    CurrentHasGround = authoring.isGrounded,
                    Resistance = authoring.resistance,
                    PathResistance = 0f,
                    ParentWire = Entity.Null
                });

                AddBuffer<ConnectedWireBuffer>(entity);
            }
        }
    }
}

