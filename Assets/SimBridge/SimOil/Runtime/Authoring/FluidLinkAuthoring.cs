using Unity.Entities;
using UnityEngine;

namespace SimOil.Tests
{
    public class FluidLinkAuthoring : MonoBehaviour
    {
        class Baker : Baker<FluidLinkAuthoring>
        {
            public override void Bake(FluidLinkAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                // Просто регистрируем компоненты для Netcode.
                // Никаких проверок на NodeA/NodeB здесь больше нет!
                AddComponent(entity, new FluidLink());
                AddComponent(entity, new PumpData());
            }
        }
    }
}