using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DotsBridge.Modules.Network
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class ClientDisconnectSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

            // 1. ОТЛОВ РАЗРЫВА ВО ВРЕМЯ ИГРЫ
            // Если у нас был тег "Очистка" (мы были в игре), но сама сущность соединения пропала
            // (или потеряла компонент NetworkStreamConnection), значит нас выкинуло.
            foreach (var (cleanup, entity) in SystemAPI.Query<RefRO<ClientConnectedCleanup>>()
                         .WithNone<NetworkStreamConnection>()
                         .WithEntityAccess())
            {
                Debug.LogWarning($"[ClientDisconnectSystem] Потеряна связь с сервером!");
                
                // Сообщаем UI
                ConnectionManager.InvokeDisconnectedFromServer("Соединение разорвано");

                // Убираем тег, чтобы не срабатывало каждый кадр
                commandBuffer.RemoveComponent<ClientConnectedCleanup>(entity);
                
                // Полная зачистка миров через мост (чтобы вернуться в меню)
                // MonoBehaviourBridge.Instance.ClearWorlds(); 
            }
            
            commandBuffer.Playback(EntityManager);
            commandBuffer.Dispose();
        }
    }
}