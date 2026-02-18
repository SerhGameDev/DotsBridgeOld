using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public class UnitAuthoring : MonoBehaviour
    {
    }

    public class UnitBaker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ID { Value = 0 }); 
        }
    }
}