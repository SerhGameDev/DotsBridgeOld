using Unity.Entities;
using UnityEngine;

public class ClientOnlyAuthoring : MonoBehaviour
{
    class Baker : Baker<ClientOnlyAuthoring>
    {
        public override void Bake(ClientOnlyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ClientOnlyTag>(entity);
        }
    }
}