#if DOTSBRIDGE_NETCODE
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

namespace DotsBridge.Modules.Network.Systems
{
    public partial class ServerUserDisconnectSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Ищем сущности, у которых остался наш CleanupState, но УЖЕ НЕТ NetworkId (значит Netcode удалил коннект)
            foreach (var (cleanup, entity) in SystemAPI.Query<RefRO<UserCleanupState>>().WithNone<NetworkId>().WithEntityAccess())
            {
                int disconnectedId = cleanup.ValueRO.NetworkId;

                // 1. Вызываем ООП-событие отключения
                DotsUserManager.RemoveUser(disconnectedId);

                // 2. Ищем и удаляем все объекты, принадлежащие этому игроку
                foreach (var (owner, ownedEntity) in SystemAPI.Query<RefRO<UserOwner>>().WithEntityAccess())
                {
                    if (owner.ValueRO.NetworkId == disconnectedId)
                    {
                        ecb.DestroyEntity(ownedEntity);
                    }
                }

                // 3. Снимаем Cleanup-компонент, чтобы окончательно удалить сущность из памяти
                ecb.RemoveComponent<UserCleanupState>(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
#endif