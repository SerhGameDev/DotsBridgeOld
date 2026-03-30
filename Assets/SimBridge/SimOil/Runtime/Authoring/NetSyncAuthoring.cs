using Unity.Entities;
using UnityEngine;
using SimOil;

namespace SimOil.Authoring
{
    // Этот скрипт гарантирует, что Netcode 100% увидит твой сетевой компонент
    [RequireComponent(typeof(Unity.NetCode.GhostAuthoringComponent))]
    public class NetSyncAuthoring : MonoBehaviour
    {
        public class Baker : Unity.Entities.Baker<NetSyncAuthoring>
        {
            public override void Bake(NetSyncAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new NetSync_FluidNode());
            }
        }
    }
}