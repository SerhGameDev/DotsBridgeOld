using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DotsBridge.Modules.Network
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class ClientAuthSystem : SystemBase
    {
        protected override void OnCreate()
        {
            // Система имеет смысл только тогда, когда мы в процессе подключения или подключены
            RequireForUpdate<NetworkId>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // --- ШАГ 1: ОТПРАВКА ЗАПРОСА ПРИ ПОДКЛЮЧЕНИИ ---
            // Ищем новые соединения, которые еще не отправили запрос
            foreach (var (networkId, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<AuthRequestSentTag>().WithEntityAccess())
            {
                // Берем логин и пароль, которые ты ввел в UI (или дефолтные из инспектора)
                var nickname = DotsBridgeBootstrapper.Instance.ActiveClientNickname;
                var password = DotsBridgeBootstrapper.Instance.ActiveClientPassword;

                // Создаем сущность сообщения RPC
                var rpcEntity = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(rpcEntity, new AuthRequestRpc
                {
                    Nickname = nickname,
                    Password = password
                });
                
                // Говорим Netcode, куда отправить это сообщение (на сервер)
                commandBuffer.AddComponent(rpcEntity, new SendRpcCommandRequest { TargetConnection = entity });

                // Вешаем тег, чтобы не отправить письмо дважды в следующем кадре
                commandBuffer.AddComponent<AuthRequestSentTag>(entity);

                Debug.Log($"[ClientAuthSystem] Физическое подключение (ID: {networkId.ValueRO.Value}). Отправка логина: {nickname}...");
            }

            // --- ШАГ 2: ЧТЕНИЕ ОТВЕТА ОТ СЕРВЕРА ---
            // Слушаем входящие AuthResponseRpc
            foreach (var (response, receiveEntity, entity) in SystemAPI.Query<RefRO<AuthResponseRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                if (response.ValueRO.Status == AuthStatus.Success)
                {
                    Debug.Log($"[ClientAuthSystem] Авторизация успешна! Добро пожаловать.");
                    ConnectionManager.InvokeConnectedToServer();
                    
                    commandBuffer.AddComponent<ClientConnectedCleanup>(receiveEntity.ValueRO.SourceConnection);
                }
                else
                {
                    Debug.LogError($"[ClientAuthSystem] Отказ в доступе: {response.ValueRO.Status}. Причина: {response.ValueRO.Reason}");
                    // Триггерим UI (показываем всплывающее окно с ошибкой)
                    ConnectionManager.InvokeDisconnectedFromServer(response.ValueRO.Reason.ToString());
                    commandBuffer.AddComponent<NetworkStreamInGame>(receiveEntity.ValueRO.SourceConnection);
                    // Если сервер нас послал, мы сами разрываем локальное соединение
                    commandBuffer.AddComponent<NetworkStreamRequestDisconnect>(receiveEntity.ValueRO.SourceConnection);
                }

                // В DOTS входящие RPC нужно удалять вручную после прочтения, иначе утечка памяти
                commandBuffer.DestroyEntity(entity);
            }

            commandBuffer.Playback(EntityManager);
            commandBuffer.Dispose();
        }
    }
}