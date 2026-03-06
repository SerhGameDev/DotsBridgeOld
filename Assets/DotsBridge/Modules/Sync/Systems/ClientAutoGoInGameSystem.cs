#if DOTSBRIDGE_NETCODE
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

namespace DotsBridge.Modules.Network.Systems
{
    // =========================================================
    // ЛОГИКА КЛИЕНТА: Рапортуем в OOP, что мы успешно вошли
    // =========================================================
    public partial class ClientAutoGoInGameSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Ищем наше собственное подключение к серверу
            foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
            {
                // Сообщаем сетевому движку, что мы готовы играть
                ecb.AddComponent<NetworkStreamInGame>(entity);

                // Вызываем наше C# событие для UI и спавна локального игрока!
                DotsUserManager.AddUser(id.ValueRO.Value);
                DotsNetworkManager.TriggerClientConnected();
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
#endif