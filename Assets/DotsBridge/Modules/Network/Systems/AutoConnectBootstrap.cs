using Unity.NetCode;
using UnityEngine;

public sealed class AutoConnectBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName) => false;
}
