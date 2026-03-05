using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        public static EntityBatch Find(string id, Allocator allocator = Allocator.Persistent)
        {
            var list = new NativeList<Entity>(allocator);
            FindInternal(GetHash(id), ref list);
            return new EntityBatch(list, Manager);
        }

        public static EntityBatch Find(string id)
        {
            var list = new NativeList<Entity>(Allocator.Temp);

            FindInternal(GetHash(id), ref list);

            return new EntityBatch(list, Manager);
        }

        public static void Find(string id, ref NativeList<Entity> results)
        {
            Find(GetHash(id), ref results);
        }

        private static void Find(int idHash, ref NativeList<Entity> results)
        {
            results.Clear();
            FindInternal(idHash, ref results);
        }

        private static void FindInternal(int idHash, ref NativeList<Entity> results)
        {
            var systemHandle = World.GetExistingSystem<IdMapSystem>();
            if (systemHandle == SystemHandle.Null) return;

            ref var map = ref World.Unmanaged.GetUnsafeSystemRef<IdMapSystem>(systemHandle).EntityMap;
            if (map.IsCreated && map.TryGetFirstValue(idHash, out Entity entity, out var iterator))
            {
                do
                {
                    results.Add(entity);
                }
                while (map.TryGetNextValue(out entity, ref iterator));
            }
        }
    }
}