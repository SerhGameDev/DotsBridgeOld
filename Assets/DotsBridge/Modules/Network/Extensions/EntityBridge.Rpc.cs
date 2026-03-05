#if DOTSBRIDGE_NETCODE
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using DotsBridge.Modules.Network;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        // =========================================================
        // ОТПРАВКА С КЛИЕНТА НА СЕРВЕР
        // =========================================================
        public static void SendRpcToServer(string eventName, int intValue = 0, float floatValue = 0, float3 vectorValue = default)
        {
            var ctx = InClientWorld();
            if (ctx.State == null) return;

            var em = ctx.State.Manager;

            // Находим наше соединение с сервером
            var connectionQuery = em.CreateEntityQuery(typeof(NetworkId), typeof(NetworkStreamInGame));
            if (connectionQuery.IsEmptyIgnoreFilter) return;

            var connectionEntity = connectionQuery.GetSingletonEntity();

            // Создаем сущность пакета и отправляем
            var rpcEntity = em.CreateEntity();
            em.AddComponentData(rpcEntity, new OopEventRpc
            {
                EventHash = GetHash(eventName),
                IntValue = intValue,
                FloatValue = floatValue,
                VectorValue = vectorValue
            });

            // Указываем, куда летит пакет
            em.AddComponentData(rpcEntity, new SendRpcCommandRequest { TargetConnection = connectionEntity });
        }

        // =========================================================
        // ОТПРАВКА С СЕРВЕРА КЛИЕНТАМ (БРОАДКАСТ)
        // =========================================================
        public static void BroadcastRpcToClients(string eventName, int intValue = 0, float floatValue = 0, float3 vectorValue = default)
        {
            var ctx = InServerWorld();
            if (ctx.State == null) return;

            var em = ctx.State.Manager;

            // Находим всех клиентов в игре
            var connectionQuery = em.CreateEntityQuery(typeof(NetworkId), typeof(NetworkStreamInGame));
            var connections = connectionQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

            var rpcData = new OopEventRpc
            {
                EventHash = GetHash(eventName),
                IntValue = intValue,
                FloatValue = floatValue,
                VectorValue = vectorValue
            };

            // Отправляем копию пакета каждому клиенту
            foreach (var clientConn in connections)
            {
                var rpcEntity = em.CreateEntity();
                em.AddComponentData(rpcEntity, rpcData);
                em.AddComponentData(rpcEntity, new SendRpcCommandRequest { TargetConnection = clientConn });
            }

            connections.Dispose();
        }
    }
}
#endif