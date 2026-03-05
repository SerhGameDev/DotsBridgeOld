using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace DotsBridge
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct ContainerCleanupSystem : ISystem
    {
        [BurstCompile]
        private struct RemoveDeadEntitiesJob : IJob
        {
            public NativeList<Entity> List;
            [ReadOnly] public EntityStorageInfoLookup StorageInfo;

            public void Execute()
            {
                for (int i = List.Length - 1; i >= 0; i--)
                {
                    if (!StorageInfo.Exists(List[i]))
                    {
                        List.RemoveAtSwapBack(i);
                    }
                }
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            if (EntityBridge.Containers.Count == 0) return;

            var storageInfo = SystemAPI.GetEntityStorageInfoLookup();

            foreach (var kvp in EntityBridge.Containers)
            {
                var container = kvp.Value;
                var list = container.Entities;

                if (!list.IsCreated || list.IsEmpty) continue;

                var job = new RemoveDeadEntitiesJob
                {
                    List = list,
                    StorageInfo = storageInfo
                };

                container.CleanupHandle = job.Schedule(container.CleanupHandle);
                state.Dependency = JobHandle.CombineDependencies(state.Dependency, container.CleanupHandle);
            }
        }
    }
}