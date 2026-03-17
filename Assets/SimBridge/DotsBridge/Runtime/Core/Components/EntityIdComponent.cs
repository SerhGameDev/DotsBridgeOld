using Unity.Entities;

namespace DotsBridge
{
    public struct EntityIdComponent : IComponentData
    {
        public int Hash;
    }
    // Вешается при спавне (например, через spawner.Spawn("Player_1"))
    public struct BridgeIdentity : IComponentData
    {
        public int Hash;
    }

    // Теневой компонент. Выживает даже после DestroyEntity!
    public struct BridgeIdentityCleanup : ICleanupComponentData
    {
        public int Hash;
    }
    public struct BridgeOwner : IComponentData
    {
        public int ClientId;
    }
}