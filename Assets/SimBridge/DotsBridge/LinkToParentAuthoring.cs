using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DotsBridge
{
    public class LinkToParentAuthoring : MonoBehaviour
    {
        [Tooltip("Перетащите сюда объект Оси (тот же самый, что указан в Target контроллера)")]
        public GameObject parentAxis;

        class Baker : Baker<LinkToParentAuthoring>
        {
            public override void Bake(LinkToParentAuthoring authoring)
            {
                if (authoring.parentAxis == null) return;

                var childEntity = GetEntity(TransformUsageFlags.Dynamic);
                
                var parentEntity = GetEntity(authoring.parentAxis, TransformUsageFlags.Dynamic);

                AddComponent(childEntity, new Parent { Value = parentEntity });
            }
        }
    }
}