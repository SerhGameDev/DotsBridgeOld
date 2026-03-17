using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
public partial class NetworkDebugSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var (driver, entity) in SystemAPI.Query<RefRO<NetworkStreamConnection>>().WithEntityAccess())
        {
        }
    }
}