using DotsBridge;
using DotsBridge.Modules.Network;
using Unity.NetCode;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //private Vector3 _lastInput;

    //private void Start()
    //{
    //    if (Unity.Entities.World.DefaultGameObjectInjectionWorld != null &&
    //        Unity.Entities.World.DefaultGameObjectInjectionWorld.IsServer())
    //    {
    //        OopRpcRegistry.SubscribeOnServer("RequestSpawn", (data) =>
    //        {
    //            int clientId = data.IntValue;

    //            string nickname = "Player_" + clientId;
    //            PlayerSessionRegistry.Register(clientId, nickname);
         
    //            EntityBridge
    //                .InClientWorld()
    //                .BeginSpawn("Cub_Server")
    //                .SetId("Player")
    //                .SetOwner(clientId)
    //                .SetPosition(new Vector3(0, 5, 0))
    //                .Spawn()
    //                .Execute();

    //            Debug.Log($"[Server] {nickname} вошел в игру и заспавнен!");
    //        });

    //        OopRpcRegistry.SubscribeOnServer("RequestMove", (data) =>
    //        {
    //            if (!PlayerSessionRegistry.HasPlayer(data.IntValue)) return;

    //            EntityBridge.GetForServer("Player")
    //                        .WithOwner(data.IntValue)
    //                        .Move(data.VectorValue, 10f);
    //        });

    //        OopRpcRegistry.SubscribeOnServer("RequestStop", (data) =>
    //        {
    //            EntityBridge.GetForServer("Player")
    //                        .WithOwner(data.IntValue)
    //                        .StopMove();
    //        });

    //        OopRpcRegistry.SubscribeOnServer("RequestDisconnect", (data) =>
    //        {
    //            EntityBridge.GetForServer("Player")
    //                        .WithOwner(data.IntValue)
    //                        .TriggerDeath();

    //            PlayerSessionRegistry.Unregister(data.IntValue);
    //            Debug.Log($"[Server] Игрок {data.IntValue} отключился.");
    //        });
    //    }
    //}

    //private void Update()
    //{
    //    if (Input.GetKeyUp(KeyCode.Space))
    //        OopRpcRegistry.SendToServer("RequestSpawn", 0);

    //    Vector3 moveInput = Vector3.zero;
    //    if (Input.GetKey(KeyCode.W)) moveInput.z += 1;
    //    if (Input.GetKey(KeyCode.S)) moveInput.z -= 1;
    //    if (Input.GetKey(KeyCode.A)) moveInput.x -= 1;
    //    if (Input.GetKey(KeyCode.D)) moveInput.x += 1;

    //    moveInput = moveInput.normalized;

    //    if (moveInput != _lastInput)
    //    {
    //        if (moveInput != Vector3.zero)
    //            OopRpcRegistry.SendToServer("RequestMove", 0, 0, moveInput);
    //        else
    //            OopRpcRegistry.SendToServer("RequestStop", 0);

    //        _lastInput = moveInput;
    //    }
    //}

    //private void OnApplicationQuit()
    //{
    //    OopRpcRegistry.SendToServer("RequestDisconnect", 0);
    //}
}