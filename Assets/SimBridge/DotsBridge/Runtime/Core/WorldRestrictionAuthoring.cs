using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DotsBridge
{
    // Теги для идентификации
    public struct ServerOnlyTag : IComponentData { }
    public struct ClientOnlyTag : IComponentData { }

    public class WorldRestrictionAuthoring : MonoBehaviour
    {
        public enum TargetWorld { Shared, ServerOnly, ClientOnly }
        
        public TargetWorld Target = TargetWorld.Shared;

        class Baker : Baker<WorldRestrictionAuthoring>
        {
            public override void Bake(WorldRestrictionAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                if (authoring.Target == TargetWorld.ServerOnly) AddComponent<ServerOnlyTag>(entity);
                else if (authoring.Target == TargetWorld.ClientOnly) AddComponent<ClientOnlyTag>(entity);
            }
        }
    }

    // Автоматический "чистильщик" мусора не из того мира
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class WorldRestrictionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (World.IsClient())
            {
                var serverOnlyQuery = SystemAPI.QueryBuilder().WithAll<ServerOnlyTag>().Build();
                EntityManager.DestroyEntity(serverOnlyQuery);
            }

            if (World.IsServer())
            { 
                var clientOnlyQuery = SystemAPI.QueryBuilder().WithAll<ClientOnlyTag>().Build();
                EntityManager.DestroyEntity(clientOnlyQuery);
            }

            Enabled = false; // Отрабатывает 1 раз при старте мира
        }
    }
}