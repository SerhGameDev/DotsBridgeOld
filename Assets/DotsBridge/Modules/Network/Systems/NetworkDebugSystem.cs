//using Unity.Entities;
//using Unity.NetCode;
//using UnityEngine;

//namespace DotsBridge.Modules.Network.Systems
//{
//    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
//    public partial class NetworkDebugSystem : SystemBase
//    {
//        protected override void OnUpdate()
//        {
//            // Мониторим все сетевые потоки
//            foreach (var (connection, entity) in SystemAPI.Query<RefRO<NetworkStreamConnection>>().WithEntityAccess())
//            {
//                // Получаем текущий статус соединения
//                var state = connection.ValueRO.CurrentState;

//                if (World.IsClient())
//                    Debug.Log($"<color=yellow>[Client-Network]</color> Статус соединения: {state}");

//                if (World.IsServer())
//                    Debug.Log($"<color=orange>[Server-Network]</color> Сервер видит соединение со статусом: {state}");
//            }
//        }
//    }
//}