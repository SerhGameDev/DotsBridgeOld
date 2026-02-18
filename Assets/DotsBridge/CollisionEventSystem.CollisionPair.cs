using Unity.Entities;

namespace DotsBridge
{
    public partial struct CollisionEventSystem
    {
        private struct CollisionPair
        {
            public Entity EntityA;
            public int IdA;
            public Entity EntityB;
            public int IdB;
        }
    }
}