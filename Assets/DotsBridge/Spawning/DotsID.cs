using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Spawning
{
    public struct SpawnRequest : IComponentData
    {
        public Entity Prefab;

        public int CountRemaining;
        public int OriginalCount; 

        public float3 Position;
        public quaternion Rotation;

        public float Scale;
        public bool OverrideScale;

        public int ID; 

        public int BatchSize;
        public float Interval;
        public float Timer;
        
        public int Loops; 
        public bool IsPaused; 
    }
}