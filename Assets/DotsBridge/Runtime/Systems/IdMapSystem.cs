using DotsBridge;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
public partial struct IdMapSystem : ISystem
{
    public NativeParallelMultiHashMap<int, Entity> EntityMap;

    public void OnCreate(ref SystemState state)
    {
        EntityMap = new NativeParallelMultiHashMap<int, Entity>(1000, Allocator.Persistent);
        state.RequireForUpdate<EntityIdComponent>();
    }

    public void OnDestroy(ref SystemState state)
    {
        if (EntityMap.IsCreated) EntityMap.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityMap.Clear();

        foreach (var (id, entity) in SystemAPI.Query<RefRO<EntityIdComponent>>().WithEntityAccess())
        {
            EntityMap.Add(id.ValueRO.Hash, entity);
        }
    }
}