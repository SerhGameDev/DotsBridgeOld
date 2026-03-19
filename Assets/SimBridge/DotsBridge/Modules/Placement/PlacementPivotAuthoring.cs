using Unity.Entities;
using UnityEngine;

namespace DotsBridge.Placement
{
    public class PlacementPivotAuthoring : MonoBehaviour
    {
        [Tooltip("Смещение от центра объекта (например, Y = 0.5 поднимет куб 1x1x1 так, чтобы он стоял на поверхности)")]
        public Vector3 Offset = new Vector3(0, 0.5f, 0); 
        
        public class PlacementPivotBaker : Baker<PlacementPivotAuthoring>
        {
            public override void Bake(PlacementPivotAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlacementPivot
                {
                    Value = authoring.Offset
                });
            }
        }
    }
}