using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DotsBridge.Modules.Network
{
// --- 3. СИСТЕМА КЛИЕНТА ---
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class ClientDisconnectSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

            // То же самое: ищем разорванные соединения клиента
            foreach (var (cleanup, entity) in SystemAPI.Query<RefRO<ClientConnectedCleanup>>().WithNone<NetworkStreamConnection>().WithEntityAccess())
            {
                Debug.LogWarning($"[ClientDisconnectSystem] Потеряна связь с сервером!");
                
                ConnectionManager.InvokeDisconnectedFromServer("Соединение разорвано");

                commandBuffer.RemoveComponent<ClientConnectedCleanup>(entity);
            }

            commandBuffer.Playback(EntityManager);
            commandBuffer.Dispose();
        }
    }
}