using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Modules.Movement
{
    public struct MoveTransformDirection : IComponentData { public float3 Value; }
    public struct MoveTransformSpeed : IComponentData { public float Value; }
    public struct IsTransformMoving : IComponentData, IEnableableComponent { }
}