using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

namespace DotsBridge.Modules.Network.Systems
{
    // =========================================================
    // ЛОГИКА КЛИЕНТА: Только переводит в статус InGame
    // =========================================================
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)] // <--- ОБЯЗАТЕЛЬНО!
    public partial class ClientAutoGoInGameSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
            {
                // Просто даем движку понять, что мы готовы играть
                ecb.AddComponent<NetworkStreamInGame>(entity);
                // УБРАЛИ ВЫЗОВ TRIGGER ОТСЮДА!
            }
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }

}