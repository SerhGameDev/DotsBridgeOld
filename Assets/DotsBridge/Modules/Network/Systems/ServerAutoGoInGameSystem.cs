using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

namespace DotsBridge.Modules.Network.Systems
{
    // =========================================================
    // ЛОГИКА СЕРВЕРА
    // =========================================================
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)] // <--- ОБЯЗАТЕЛЬНО!
    public partial class ServerAutoGoInGameSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
            {
                ecb.AddComponent<NetworkStreamInGame>(entity);
                UnityEngine.Debug.Log($"[DotsBridge-Server] Клиент {id.ValueRO.Value} одобрен и вошел в игру.");
            }
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }

}