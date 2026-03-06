#if DOTSBRIDGE_NETCODE
using Unity.Mathematics;
using Unity.NetCode;
using DotsBridge.Modules.Network;

namespace DotsBridge
{
    public static partial class NetEntityBridge 
    {
        // =========================================================
        // ОТПРАВКА С КЛИЕНТА НА СЕРВЕР
        // =========================================================
        public static void SendRpcToServer(string eventName, int intValue = 0, float floatValue = 0, float3 vectorValue = default)
        {
            var ctx = EntityBridge.ClientState;
            if (ctx == null) return;

            var em = ctx.Manager;

            var connectionQuery = em.CreateEntityQuery(typeof(NetworkId), typeof(NetworkStreamInGame));
            if (connectionQuery.IsEmptyIgnoreFilter) return;

            var connectionEntity = connectionQuery.GetSingletonEntity();

            var rpcEntity = em.CreateEntity();
            em.AddComponentData(rpcEntity, new OopEventRpc
            {
                EventHash = EntityBridge.GetHash(eventName),
                IntValue = intValue,
                FloatValue = floatValue,
                VectorValue = vectorValue
            });

            em.AddComponentData(rpcEntity, new SendRpcCommandRequest { TargetConnection = connectionEntity });
        }

        // =========================================================
        // ОТПРАВКА С СЕРВЕРА КЛИЕНТАМ (БРОАДКАСТ)
        // =========================================================
        public static void BroadcastRpcToClients(string eventName, int intValue = 0, float floatValue = 0, float3 vectorValue = default)
        {
            var ctx = EntityBridge.ServerState;
            if (ctx == null) return;

            var em = ctx.Manager;

            var connectionQuery = em.CreateEntityQuery(typeof(NetworkId), typeof(NetworkStreamInGame));
            var connections = connectionQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

            var rpcData = new OopEventRpc
            {
                EventHash = EntityBridge.GetHash(eventName),
                IntValue = intValue,
                FloatValue = floatValue,
                VectorValue = vectorValue
            };

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