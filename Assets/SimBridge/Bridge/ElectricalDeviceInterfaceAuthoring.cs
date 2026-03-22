using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace SimElectric.Bridge
{
    public class ElectricalDeviceInterfaceAuthoring : MonoBehaviour
    {
        [Title("Electrical Connection")]
        [Required("Ссылка на клемму питания двигателя")]
        public ElectricalNodeAuthoring powerTerminal;
        
        [Tooltip("Минимальное напряжение для запуска")]
        public float activationVoltage = 24f;

        public class Baker : Baker<ElectricalDeviceInterfaceAuthoring>
        {
            public override void Bake(ElectricalDeviceInterfaceAuthoring authoring)
            {
                if (authoring.powerTerminal == null) return;
                
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ElectricalDeviceInterface
                {
                    PowerTerminal = GetEntity(authoring.powerTerminal, TransformUsageFlags.Dynamic),
                    ActivationVoltage = authoring.activationVoltage,
                    IsPowered = false
                });
            }
        }
    }
}