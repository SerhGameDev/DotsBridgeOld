using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DotsBridge.Modules.Network
{
  // --- 1. CLEANUP КОМПОНЕНТЫ ---
    // Они остаются на сущности, даже когда Netcode её уничтожает при разрыве связи
    
    public struct SessionCleanup : ICleanupComponentData
    {
        public int NetworkId;
    }

    public struct ClientConnectedCleanup : ICleanupComponentData { }


    // --- 2. СИСТЕМА СЕРВЕРА ---
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ServerDisconnectSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

            // Ищем сущности, у которых ЕСТЬ наш SessionCleanup, но УЖЕ НЕТ NetworkStreamConnection
            // Это означает, что Netcode разорвал связь и попытался удалить эту сущность.
            foreach (var (cleanup, entity) in SystemAPI.Query<RefRO<SessionCleanup>>().WithNone<NetworkStreamConnection>().WithEntityAccess())
            {
                int id = cleanup.ValueRO.NetworkId;

                if (ConnectionManager.Players.TryGetValue(id, out var session))
                {
                    Debug.Log($"[ServerDisconnectSystem] Игрок {session.Nickname} (ID: {id}) отключился.");
                    
                    ConnectionManager.InvokePlayerLeft(id, "Соединение разорвано");

                    if (session.ProfileEntity != Entity.Null && EntityManager.Exists(session.ProfileEntity))
                    {
                        commandBuffer.DestroyEntity(session.ProfileEntity);
                        Debug.Log($"[ServerDisconnectSystem] Аватар игрока {session.Nickname} удален.");
                    }

                    ConnectionManager.Players.Remove(id);
                }

                // Снимаем Cleanup-компонент, разрешая DOTS окончательно удалить эту сущность из памяти
                commandBuffer.RemoveComponent<SessionCleanup>(entity);
            }

            commandBuffer.Playback(EntityManager);
            commandBuffer.Dispose();
        }
    }
}