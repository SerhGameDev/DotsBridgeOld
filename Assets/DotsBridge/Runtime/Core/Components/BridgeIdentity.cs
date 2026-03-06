using Unity.Entities;

namespace DotsBridge
{
    // 1. Основной компонент. Вешается при спавне.
    public struct BridgeIdentity : IComponentData
    {
        public int Hash;
    }

    // 2. Теневой компонент (Cleanup). 
    // Он выживает, даже если вызвать EntityManager.DestroyEntity()!
    public struct BridgeIdentityCleanup : ICleanupComponentData
    {
        public int Hash;
    }
}