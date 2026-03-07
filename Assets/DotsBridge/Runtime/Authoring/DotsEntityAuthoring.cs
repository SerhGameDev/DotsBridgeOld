using DotsBridge.Modules.Movement;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge.Authoring
{
    [DisallowMultipleComponent]
    public class DotsEntityAuthoring : MonoBehaviour
    {
        [Header("DotsBridge Identity")]
        public string Id;

        [Header("Base Movement Settings")]
        public float MoveSpeed = 5f;
        public bool IsMovingInitially = false;

        public class Baker : Baker<DotsEntityAuthoring>
        {
            public override void Bake(DotsEntityAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                int hash = string.IsNullOrEmpty(authoring.Id) ? 0 : EntityBridge.GetHash(authoring.Id);

                AddComponent(entity, new BridgeIdentity { Hash = hash });

                AddComponent(entity, new BridgeOwner { ClientId = 0 });

                AddComponent(entity, new DeathEvent());
                AddComponent(entity, new IsTransformMoving());
                AddComponent(entity, new MoveTransformSpeed { Value = authoring.MoveSpeed });
                AddComponent(entity, new MoveTransformDirection());

                SetComponentEnabled<DeathEvent>(entity, false);
                SetComponentEnabled<IsTransformMoving>(entity, authoring.IsMovingInitially);
            }
        }
    }
}