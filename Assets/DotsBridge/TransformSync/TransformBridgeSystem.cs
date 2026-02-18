using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial class TransformBridgeSystem : SystemBase
    {
        protected override void OnUpdate()
        { 
            foreach (var (transformRef, link, settings) in SystemAPI.Query<RefRW<LocalTransform>, LinkedTransform, RefRO<SyncSettings>>())
            {
                Transform goTransform = link.Transform;

                if (goTransform == null) continue; 

                if (settings.ValueRO.SyncMode == SyncSettings.Mode.CopyFromGameObject)
                {
                    // GameObject -> Entity
                    if (settings.ValueRO.SyncPosition)
                        transformRef.ValueRW.Position = goTransform.position;

                    if (settings.ValueRO.SyncRotation)
                        transformRef.ValueRW.Rotation = goTransform.rotation;
                }
                else
                {
                    // Entity -> GameObject
                    if (settings.ValueRO.SyncPosition)
                        goTransform.position = transformRef.ValueRO.Position;

                    if (settings.ValueRO.SyncRotation)
                        goTransform.rotation = transformRef.ValueRO.Rotation;
                }
            }
        }
    }
}