using SimVent.Components;
using Unity.Entities;
using UnityEngine;

namespace SimVent.Authoring
{
    public class FanBladesAuthoring : MonoBehaviour
    {
        class Baker : Baker<FanBladesAuthoring>
        {
            public override void Bake(FanBladesAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new FanMaterialSpeedComponent { Value = 0f });
            }
        }
    }
}