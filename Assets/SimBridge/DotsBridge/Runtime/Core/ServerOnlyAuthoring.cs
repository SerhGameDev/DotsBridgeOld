using Unity.Entities;
using UnityEngine;

public class ServerOnlyAuthoring : MonoBehaviour
{
    class Baker : Baker<ServerOnlyAuthoring>
    {
        public override void Bake(ServerOnlyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ServerOnlyTag>(entity);
        }
    }
}