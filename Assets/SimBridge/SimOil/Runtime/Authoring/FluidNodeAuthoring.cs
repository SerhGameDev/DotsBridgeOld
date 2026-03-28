using Unity.Entities;
using UnityEngine;

namespace SimOil.Tests
{
    public class FluidNodeAuthoring : MonoBehaviour
    {
        class Baker : Baker<FluidNodeAuthoring>
        {
            public override void Bake(FluidNodeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new FluidMixture()); 
            }
        }
    }

    public struct FluidSpawnerConfig : IComponentData
    {
        public Entity NodePrefab;
        public Entity LinkPrefab;
    }
}