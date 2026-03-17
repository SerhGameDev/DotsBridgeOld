using Unity.Entities;

namespace DotsBridge
{
    /// <summary>
    /// Внутренняя система, которая внедряется в каждый BridgeWorld.
    /// Отслеживает момент уничтожения мира движком Unity.
    /// </summary>
    [DisableAutoCreation] 
    public partial class BridgeCleanupSystem : SystemBase
    {
        protected override void OnUpdate()
        {
        }

        protected override void OnDestroy()
        {
            EntityBridge.HandleWorldDestroyed(World);
        }
    }
}