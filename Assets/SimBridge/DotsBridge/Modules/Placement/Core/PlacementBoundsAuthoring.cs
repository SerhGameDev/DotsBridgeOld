using Unity.Entities;
using UnityEngine;

namespace DotsBridge.Placement
{
    public class PlacementBoundsAuthoring : MonoBehaviour
    {
        [Tooltip("Размер виртуального Box-коллайдера для проверки наложений")]
        public Vector3 Size = new Vector3(1, 1, 1);
        
        // Рисуем Gizmos в редакторе Unity для удобной настройки
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawWireCube(transform.position, Size);
        }
        
        public class PlacementBoundsBaker : Baker<PlacementBoundsAuthoring>
        {
            public override void Bake(PlacementBoundsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlacementBounds { Size = authoring.Size });
            }
        }
    }
}