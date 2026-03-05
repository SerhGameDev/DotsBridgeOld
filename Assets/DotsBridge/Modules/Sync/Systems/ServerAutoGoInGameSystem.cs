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

            // Ищем все новые соединения (NetworkId), у которых еще нет статуса InGame
            foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
            {
                // Разрешаем этому соединению участвовать в игре (спавнить и синхронизировать объекты)
                ecb.AddComponent<NetworkStreamInGame>(entity);
                UnityEngine.Debug.Log($"[DotsBridge-Server] Клиент {id.ValueRO.Value} одобрен и вошел в игру.");
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }

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
                DotsNetworkManager.TriggerClientConnected();
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
#endif