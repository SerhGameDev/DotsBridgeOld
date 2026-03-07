using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

namespace DotsBridge.Modules.Network.Systems
{// =========================================================
    // МОСТ В ООП: Ловит тех, кто InGame, и дергает UI
    // =========================================================
    public struct ConnectionEventTriggered : IComponentData { }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)] // <--- Слушаем только на клиенте
    public partial class NetworkEventBridgeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (netId, stream, entity) in SystemAPI.Query<RefRO<NetworkId>, RefRO<NetworkStreamInGame>>()
                         .WithNone<ConnectionEventTriggered>()
                         .WithEntityAccess())
            {
                // 1. Вызываем статическое OOP-событие ровно 1 раз
                DotsNetworkManager.TriggerClientConnected();

                // 2. Ставим маркер, чтобы не вызывать событие каждый кадр
                ecb.AddComponent<ConnectionEventTriggered>(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}