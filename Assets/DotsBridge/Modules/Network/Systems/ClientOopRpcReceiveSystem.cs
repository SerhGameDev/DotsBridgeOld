#if DOTSBRIDGE_NETCODE
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

namespace DotsBridge.Modules.Network.Systems
{
    // Система, ловящая RPC на клиенте
    public partial class ClientOopRpcReceiveSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (rpc, req, entity) in SystemAPI.Query<RefRO<OopEventRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                OopRpcRegistry.InvokeOnClient(rpc.ValueRO.EventHash, rpc.ValueRO);
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
#endif