using System;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        internal static EntityBatch GetByComponentsInternal(BridgeState state, ComponentType[] types)
        {
            if (state == null) return default;

            var queryDesc = new EntityQueryDesc { All = types };
            var query = state.Manager.CreateEntityQuery(queryDesc);

            var entityArray = query.ToEntityArray(Allocator.Temp);

            var entityList = new NativeList<Entity>(entityArray.Length, Allocator.Persistent);
            entityList.AddRange(entityArray);

            entityArray.Dispose();
            return new EntityBatch(entityList, state);
        }
    }
}