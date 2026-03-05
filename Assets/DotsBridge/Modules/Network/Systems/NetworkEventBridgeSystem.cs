#if DOTSBRIDGE_NETCODE
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

namespace DotsBridge.Modules.Network.Systems
{
    public struct ConnectionEventTriggered : IComponentData { }

    public partial class NetworkEventBridgeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Создаем временный буфер команд, так как добавление компонента напрямую в цикле вызовет ошибку
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Современный и быстрый поиск через SystemAPI
            foreach (var (netId, stream, entity) in SystemAPI.Query<RefRO<NetworkId>, RefRO<NetworkStreamInGame>>()
                         .WithNone<ConnectionEventTriggered>()
                         .WithEntityAccess())
            {
                // 1. Вызываем статическое OOP-событие
                DotsNetworkManager.TriggerClientConnected();

                // 2. Записываем команду на добавление маркера
                ecb.AddComponent<ConnectionEventTriggered>(entity);
            }

            // Выполняем все записанные команды разом
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
#endif