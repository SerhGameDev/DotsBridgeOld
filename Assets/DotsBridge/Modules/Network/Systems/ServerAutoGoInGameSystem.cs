#if DOTSBRIDGE_NETCODE
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

namespace DotsBridge.Modules.Network.Systems
{
    // =========================================================
    // ЛОГИКА СЕРВЕРА: Автоматически пускаем всех клиентов в игру
    // =========================================================
    public partial class ServerAutoGoInGameSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
            {
                ecb.AddComponent<NetworkStreamInGame>(entity);
                DotsUserManager.AddUser(id.ValueRO.Value);

                ecb.AddComponent(entity, new UserCleanupState { NetworkId = id.ValueRO.Value });
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
#endif