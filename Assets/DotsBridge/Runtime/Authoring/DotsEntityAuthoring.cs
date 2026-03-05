using DotsBridge.Modules.Movement;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge.Authoring
{
    /// <summary>
    /// Базовый компонент для всех объектов, которые будут управляться через DotsBridge.
    /// Вешается на префабы GameObject.
    /// </summary>
    [DisallowMultipleComponent]
    public class DotsEntityAuthoring : MonoBehaviour
    {
        [Header("DotsBridge Identity")]
        public string Id; // Если пусто, сгенерирует 0

        [Header("Base Movement Settings")]
        public float MoveSpeed = 5f;
        public bool IsMovingInitially = false;

        public class Baker : Baker<DotsEntityAuthoring>
        {
            public override void Bake(DotsEntityAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Запекаем ID
                int hash = string.IsNullOrEmpty(authoring.Id) ? 0 : EntityBridge.GetHash(authoring.Id);
                AddComponent(entity, new EntityIdComponent { Hash = hash });

                // Запекаем базовое движение
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