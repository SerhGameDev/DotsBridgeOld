using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge
{
    public struct SpawnRequest : IComponentData
    {
        public Entity Prefab;
        public int Count;

        public float3 Position;
        public quaternion Rotation;

        public float Scale;
        public bool OverrideScale;

        public int ID;
        public int OwnerID;
    }
}