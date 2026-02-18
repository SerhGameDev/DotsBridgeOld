using DotsBridge.Timeline;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    public static partial class Dots
    {

        public static RotationGroupBuilder RotationSequence(NativeArray<Entity> entities)
        {
            return new RotationGroupBuilder(Manager, entities, Allocator.Temp);
        }

        public static RotationGroupBuilder RotationSequence(NativeList<Entity> entities)
        {
            return new RotationGroupBuilder(Manager, entities.AsArray(), Allocator.Temp);
        }

        public static RotationGroupBuilder RotationSequence(Entity entity)
        {
            var arr = new NativeArray<Entity>(1, Allocator.Temp);
            arr[0] = entity;
            return new RotationGroupBuilder(Manager, arr, Allocator.Temp);
        }
    }
}