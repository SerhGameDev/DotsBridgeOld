using Unity.Entities;
using UnityEngine;
using SimOil.Network;

namespace SimOil.Authoring
{
    public class NetSync_FluidMixtureAuthoring : MonoBehaviour
    {
        [Tooltip("Ссылка на GameObject (Узел), откуда брать тяжелые данные симуляции")]
        public GameObject targetSimulationNode;

        public class Baker : Baker<NetSync_FluidMixtureAuthoring>
        {
            public override void Bake(NetSync_FluidMixtureAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic); 
                
                AddComponent<NetSync_FluidMixture>(entity);

                if (authoring.targetSimulationNode != null)
                {
                    AddComponent(entity, new SimTargetLink
                    {
                        TargetEntity = GetEntity(authoring.targetSimulationNode, TransformUsageFlags.None)
                    });
                }
            }
        }
    }
}