using DotsBridge.Build;
using DotsBridge.Modules.Movement;
using DotsBridge.Modules.Rotation;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge.Authoring
{
    [DisallowMultipleComponent]
    public class DotsEntityAuthoring : MonoBehaviour
    {
        [Header("DotsBridge Identity")]
        public string Id;

        [Header("Hierarchy Settings")]
        [Tooltip("Если включено, все дочерние объекты станут динамическими сущностями и будут следовать за родителем.")]
        public bool BakeChildrenAsDynamic = true;

        [Header("Movement Configuration")]
        public bool UseMovement = true;
        public float MoveSpeed = 5f;
        public bool IsMovingInitially = false;

        [Header("Rotation Configuration")]
        public bool UseRotation = false;
        public Space RotationSpace = Space.Self;
        public Vector3 RotationAxis = Vector3.up;
        public float RotationSpeed = 2f;
        public bool IsRotatingInitially = false;

        [Header("Mouse Rotation Settings")]
        public bool EnableMouseRotate = false;
        public bool StartActivated = true;
        public float Sensitivity = 0.5f;

        [Header("Inertia")]
        public bool UseInertia = true;
        [Range(0.8f, 0.99f)] public float Friction = 0.95f;

        [Header("Interaction Settings")]
        public bool IsInteractable = false;

        public class Baker : Baker<DotsEntityAuthoring>
        {
            public override void Bake(DotsEntityAuthoring authoring)
            {
                // Запекаем самого родителя как динамическую сущность
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // --- 0. ИЕРАРХИЯ (ЗАПЕКАНИЕ ДЕТЕЙ) ---
                if (authoring.BakeChildrenAsDynamic)
                {
                    // Находим всех детей в иерархии GameObject
                    var children = authoring.GetComponentsInChildren<Transform>();
                    foreach (var child in children)
                    {
                        // Пропускаем самого себя, чтобы не зациклиться
                        if (child == authoring.transform) continue;

                        // Регистрация ребенка с флагом Dynamic автоматически создает 
                        // компоненты Parent и LocalTransform в ECS мире.
                        GetEntity(child, TransformUsageFlags.Dynamic);
                    }
                }

                // --- 1. IDENTITY & CORE ---
                int hash = string.IsNullOrEmpty(authoring.Id) ? 0 : EntityBridge.GetHash(authoring.Id);
                AddComponent(entity, new BridgeIdentity { Hash = hash });
                AddComponent(entity, new BridgeOwner { ClientId = 0 });
                AddComponent(entity, new DeathEvent());
                SetComponentEnabled<DeathEvent>(entity, false);
                AddComponent<WallTag>(entity);
                AddComponent<WallComponent>(entity);
                
                // Добавляем буфер для связей
                AddBuffer<ConnectionElement>(entity);
                // --- 2. MOVEMENT ---
                if (authoring.UseMovement)
                {
                    AddComponent(entity, new MoveTransformSpeed { Value = authoring.MoveSpeed });
                    AddComponent(entity, new MoveTransformDirection { Value = float3.zero });
                    AddComponent(entity, new IsTransformMoving());
                    SetComponentEnabled<IsTransformMoving>(entity, authoring.IsMovingInitially);
                }

                // --- 3. ROTATION (AUTO) ---
                if (authoring.UseRotation)
                {
                    float3 axis = (float3)authoring.RotationAxis;
                    if (math.lengthsq(axis) > 0.0001f) axis = math.normalize(axis);
                    else axis = float3.zero;

                    AddComponent(entity, new RotateTransformAxis
                    {
                        Value = axis,
                        IsLocal = authoring.RotationSpace == Space.Self
                    });

                    AddComponent(entity, new RotateTransformSpeed { Value = authoring.RotationSpeed });
                    AddComponent(entity, new IsTransformRotating());
                    SetComponentEnabled<IsTransformRotating>(entity, authoring.IsRotatingInitially);
                }

                // --- 4. MOUSE ROTATION (INSPECTION) ---
                if (authoring.EnableMouseRotate)
                {
                    AddComponent(entity, new MouseRotationConfig
                    {
                        Sensitivity = authoring.Sensitivity,
                        UseInertia = authoring.UseInertia,
                        Friction = authoring.Friction
                    });
                    AddComponent(entity, new MouseRotationVelocity());
                    AddComponent(entity, new IsCurrentlyDragging());
                    AddComponent(entity, new CanBeMouseRotated());

                    SetComponentEnabled<IsCurrentlyDragging>(entity, false);
                    SetComponentEnabled<CanBeMouseRotated>(entity, authoring.StartActivated);
                }

                // --- 5. INTERACTION ---
                if (authoring.IsInteractable)
                {
                    AddComponent(entity, new InteractableTag());
                }
            }
        }
    }
}