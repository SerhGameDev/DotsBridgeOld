using DotsBridge.Timeline;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace DotsBridge
{
    public static partial class Dots
    {
        public static JobHandle MapDependency;

        public static NativeList<Entity> Find(string groupID, Allocator allocator)
        {
            var result = new NativeList<Entity>(allocator);
            var manager = Manager;

            MapDependency.Complete();

            var singletonQuery = manager.CreateEntityQuery(typeof(EntityIdMap));
            if (singletonQuery.IsEmptyIgnoreFilter) return result;

            var map = singletonQuery.GetSingleton<EntityIdMap>().Map;
            int targetHash = new FixedString32Bytes(groupID).GetHashCode();

            if (map.TryGetFirstValue(targetHash, out Entity entity, out var iterator))
            {
                do { result.Add(entity); }
                while (map.TryGetNextValue(out entity, ref iterator));
            }

            return result;
        }

        public static RelativeGroupBuilder Find(string groupID)
        {
            return Sequence(Find(groupID, Allocator.Temp));
        }
    }
}
