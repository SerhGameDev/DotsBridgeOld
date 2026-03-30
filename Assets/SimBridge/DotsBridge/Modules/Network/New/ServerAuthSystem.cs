using DotsBridge.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DotsBridge.Modules.Network
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ServerAuthSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            var bootstrapper = MonoBehaviourBridge.Instance;

            if (bootstrapper == null) return;

            foreach (var (request, receiveInfo, entity) in SystemAPI.Query<RefRO<AuthRequestRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                var connectionEntity = receiveInfo.ValueRO.SourceConnection;
                var nickname = request.ValueRO.Nickname.ToString();
                var password = request.ValueRO.Password.ToString();
                
                int networkId = -1;
                if (SystemAPI.HasComponent<NetworkId>(connectionEntity))
                {
                    networkId = SystemAPI.GetComponent<NetworkId>(connectionEntity).Value;
                }

                Debug.Log($"[ServerAuthSystem] Входящий запрос от {nickname} (ID: {networkId})");

                AuthStatus finalStatus = AuthStatus.Success;
                string rejectReason = "";

                // --- 1. ПРОВЕРКА ПАРОЛЯ (теперь через .Security) ---
                if (!string.IsNullOrEmpty(bootstrapper.Security.ServerPassword) && password != bootstrapper.Security.ServerPassword)
                {
                    finalStatus = AuthStatus.WrongPassword;
                    rejectReason = "Неверный пароль сервера.";
                }
                // --- 2. ПРОВЕРКА ЛИМИТА ИГРОКОВ (теперь через .Security) ---
                else if (ConnectionManager.Players.Count >= bootstrapper.Security.MaxPlayers)
                {
                    finalStatus = AuthStatus.ServerFull;
                    rejectReason = "Сервер переполнен.";
                }
                // --- 3. ПРОВЕРКА АВТО-ВХОДА (теперь через .Security) ---
                else if (!bootstrapper.Security.AutoAcceptConnections)
                {
                    finalStatus = AuthStatus.Rejected;
                    rejectReason = "Требуется ручное подтверждение хостом.";
                    
                    var authData = new AuthRequestData { NetworkId = networkId, Nickname = nickname, Password = password, ConnectionEntity = connectionEntity };
                    ConnectionManager.InvokePlayerRequestJoin(authData);
                }

                // --- 4. ОТПРАВКА ОТВЕТА ---
                var responseRpc = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(responseRpc, new AuthResponseRpc
                {
                    Status = finalStatus,
                    Reason = rejectReason
                });
                commandBuffer.AddComponent(responseRpc, new SendRpcCommandRequest { TargetConnection = connectionEntity });

                // --- 5. РЕГИСТРАЦИЯ ИГРОКА ---
                if (finalStatus == AuthStatus.Success)
                {
                    commandBuffer.AddComponent<NetworkStreamInGame>(connectionEntity);
                    commandBuffer.AddComponent(connectionEntity, new SessionCleanup { NetworkId = networkId });

                    var session = new PlayerSession
                    {
                        NetworkId = networkId,
                        Nickname = nickname,
                        ConnectionEntity = connectionEntity
                    };

                    ConnectionManager.Players[networkId] = session;
                    ConnectionManager.InvokePlayerJoined(session);
                    
                    Debug.Log($"[ServerAuthSystem] Игрок {nickname} успешно добавлен.");
                }
                else
                {
                    Debug.LogWarning($"[ServerAuthSystem] Отказ игроку {nickname}. Причина: {finalStatus}");
                    commandBuffer.AddComponent<NetworkStreamRequestDisconnect>(connectionEntity);
                }

                commandBuffer.DestroyEntity(entity);
            }

            commandBuffer.Playback(EntityManager);
            commandBuffer.Dispose();
        }
    }
}