using DotsBridge;
using DotsBridge.Modules.Network;
using UnityEngine;

public class DemoClient : MonoBehaviour
{
    private void Awake()
    {
        OopRpcRegistry.SubscribeOnClient("WaveStarted", rpc => {
            Debug.Log($"Началась волна {rpc.IntValue}");
        });
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EntityBridge.SendRpcToServer("SpawnCube");
        }
    }
}
