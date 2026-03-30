using SimOil.Network;
using Unity.Entities;
using UnityEngine;

namespace SimOil.Authoring
{
    public class NetSync_FluidLinkAuthoring : MonoBehaviour
    {
        public GameObject targetSimulationPipe;

        public class Baker : Baker<NetSync_FluidLinkAuthoring>
        {
            public override void Bake(NetSync_FluidLinkAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<NetSync_FluidLink>(entity);

                if (authoring.targetSimulationPipe != null)
                {
                    AddComponent(entity, new SimTargetLink
                    {
                        TargetEntity = GetEntity(authoring.targetSimulationPipe, TransformUsageFlags.None)
                    });
                }
            }
        }
    }
}