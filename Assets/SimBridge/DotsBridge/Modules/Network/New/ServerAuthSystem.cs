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
            var bootstrapper = DotsBridgeBootstrapper.Instance;

            // Если синглтон настроек еще не проснулся, просто ждем
            if (bootstrapper == null) return;

            // Ищем все входящие запросы на авторизацию
            foreach (var (request, receiveInfo, entity) in SystemAPI.Query<RefRO<AuthRequestRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                var connectionEntity = receiveInfo.ValueRO.SourceConnection;
                var nickname = request.ValueRO.Nickname.ToString();
                var password = request.ValueRO.Password.ToString();
                
                // Получаем ID сокета
                int networkId = -1;
                if (SystemAPI.HasComponent<NetworkId>(connectionEntity))
                {
                    networkId = SystemAPI.GetComponent<NetworkId>(connectionEntity).Value;
                }

                Debug.Log($"[ServerAuthSystem] Входящий запрос от {nickname} (ID: {networkId})");

                AuthStatus finalStatus = AuthStatus.Success;
                string rejectReason = "";

                // --- 1. ПРОВЕРКА ПАРОЛЯ ---
                if (!string.IsNullOrEmpty(bootstrapper.ServerPassword) && password != bootstrapper.ServerPassword)
                {
                    finalStatus = AuthStatus.WrongPassword;
                    rejectReason = "Неверный пароль сервера.";
                }
                // --- 2. ПРОВЕРКА ЛИМИТА ИГРОКОВ ---
                else if (ConnectionManager.Players.Count >= bootstrapper.MaxPlayers)
                {
                    finalStatus = AuthStatus.ServerFull;
                    rejectReason = "Сервер переполнен.";
                }
                // --- 3. ПРОВЕРКА АВТО-ВХОДА ---
                else if (!bootstrapper.AutoAcceptConnections)
                {
                    // Если галочка снята, мы пока просто отклоняем вход и кидаем ивент.
                    // (В будущем здесь можно сделать добавление в очередь ожидания)
                    finalStatus = AuthStatus.Rejected;
                    rejectReason = "Требуется ручное подтверждение хостом.";
                    
                    var authData = new AuthRequestData { NetworkId = networkId, Nickname = nickname, Password = password, ConnectionEntity = connectionEntity };
                    ConnectionManager.InvokePlayerRequestJoin(authData);
                }

                // --- 4. ОТПРАВКА ОТВЕТА КЛИЕНТУ ---
                var responseRpc = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(responseRpc, new AuthResponseRpc
                {
                    Status = finalStatus,
                    Reason = rejectReason
                });
                commandBuffer.AddComponent(responseRpc, new SendRpcCommandRequest { TargetConnection = connectionEntity });

                // --- 5. РЕГИСТРАЦИЯ ИГРОКА (ЕСЛИ УСПЕХ) ---
                if (finalStatus == AuthStatus.Success)
                {
                    commandBuffer.AddComponent<NetworkStreamInGame>(connectionEntity);
                    
                    // ДОБАВЛЯЕМ СЮДА НАШ CLEANUP ДЛЯ СЕРВЕРА:
                    commandBuffer.AddComponent(connectionEntity, new SessionCleanup { NetworkId = networkId });

                    var session = new PlayerSession
                    {
                        NetworkId = networkId,
                        Nickname = nickname,
                        ConnectionEntity = connectionEntity
                    };

                    ConnectionManager.Players[networkId] = session;
                    ConnectionManager.InvokePlayerJoined(session); // Триггерим UI сервера
                    
                    
                    Debug.Log($"[ServerAuthSystem] Игрок {nickname} (ID: {networkId}) успешно добавлен. Всего игроков: {ConnectionManager.Players.Count}");
                }
                else
                {
                    Debug.LogWarning($"[ServerAuthSystem] Отказ игроку {nickname}. Причина: {finalStatus}");
                    
                    // Если отказали, сервер сам вешает запрос на разрыв связи. 
                    // Клиент, получив ответ, сделает то же самое. Двойная надежность.
                    commandBuffer.AddComponent<NetworkStreamRequestDisconnect>(connectionEntity);
                }

                // В DOTS входящие RPC нужно удалять вручную после прочтения!
                commandBuffer.DestroyEntity(entity);
            }

            commandBuffer.Playback(EntityManager);
            commandBuffer.Dispose();
        }
    }
}