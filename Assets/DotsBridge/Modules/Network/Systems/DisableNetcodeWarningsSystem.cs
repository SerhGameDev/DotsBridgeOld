#if UNITY_EDITOR
using Unity.Entities;
using Unity.NetCode;

namespace DotsBridge.Modules.Network.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct DisableNetcodeWarningsSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { }
        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            SystemHandle warningSystemHandle = state.World.GetExistingSystem<WarnAboutBatchedTicksSystem>();
            if (warningSystemHandle != SystemHandle.Null)
            {
                ref SystemState targetState = ref state.WorldUnmanaged.ResolveSystemStateRef(warningSystemHandle);
                targetState.Enabled = false;
            }
            state.Enabled = false;
        }
    }
}
#endif