using Unity.Entities;
using UnityEngine;

namespace DotsBridge.Placement
{
    public struct PlacementLayer : IComponentData
    {
        public int Hash;
      
    }
    public class PlacementLayerAuthoring : MonoBehaviour
    {
        [Tooltip("Слой для фильтрации размещения (например: 'Cabinet', 'Wall', 'Floor')")]
        public string LayerName = "Default";

        public class PlacementLayerBaker : Baker<PlacementLayerAuthoring>
        {
            public override void Bake(PlacementLayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                // Превращаем строку в int хэш для молниеносного сравнения в ECS
                AddComponent(entity, new PlacementLayer 
                { 
                    Hash = EntityBridge.GetHash(authoring.LayerName) 
                });
            }
        }
    }
 
}