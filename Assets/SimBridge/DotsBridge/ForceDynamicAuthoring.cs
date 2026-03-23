using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    // Навесьте этот скрипт на каждую лопасть или дочерний объект, который должен вращаться
    public class ForceDynamicAuthoring : MonoBehaviour
    {
        class Baker : Baker<ForceDynamicAuthoring>
        {
            public override void Bake(ForceDynamicAuthoring authoring)
            {
                GetEntity(TransformUsageFlags.Dynamic);
            }
        }
    }
}