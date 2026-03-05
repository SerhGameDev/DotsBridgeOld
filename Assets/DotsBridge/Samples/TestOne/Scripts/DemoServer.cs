using DotsBridge;
using DotsBridge.Modules.Network;
using UnityEngine;

public class DemoServer : MonoBehaviour
{
    private void Start()
    {
        EntityBridge.BroadcastRpcToClients("WaveStarted", intValue: 5);
    }
    private void OnEnable()
    {
        OopRpcRegistry.SubscribeOnServer("SpawnCube", HandleSpawnRequest);
    }

    private void HandleSpawnRequest(OopEventRpc data)
    {
        Debug.Log("Клиент попросил заспавнить куб!");
        EntityBridge.InServerWorld().BeginSpawn("Cub").SetPosition(new Vector3(0, 5, 0)).SpawnAsync();
    }
}