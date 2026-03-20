using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DotsBridge.Placement
{
    public class StretchableDuctAuthoring : MonoBehaviour
    {
        [Tooltip("Ширина короба (X)")]
        public float Width = 0.1f;
        [Tooltip("Высота короба (Y)")]
        public float Height = 0.1f;

        public class DuctBaker : Baker<StretchableDuctAuthoring>
        {
            public override void Bake(StretchableDuctAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new StretchableDuct 
                { 
                    Width = authoring.Width, 
                    Height = authoring.Height,
                    IsDrawing = false 
                });
                
                // ВАЖНО: Этот компонент позволяет масштабировать куб неравномерно (например, только по Z)
                AddComponent<PostTransformMatrix>(entity);
            }
        }
    }
}