using Unity.Entities;

namespace DotsBridge
{
    public struct EntityIdComponent : IComponentData
    {
        public int Hash;
    }
}