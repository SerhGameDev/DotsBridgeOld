using DotsBridge.Modules.Rotation;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace DotsBridge.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.Default)]
    public partial class MouseHoverSystem : SystemBase
    {
        private Entity _lastHoveredEntity = Entity.Null;

        public float CheckInterval = 0.05f;
        private float _timer = 0f;

        // Позволяет настроить маску слоев. По умолчанию сталкивается со всем.
        // Чтобы игнорировать пол/стены, здесь нужно будет задать конкретные BelongsTo/CollidesWith маски.
        public CollisionFilter HoverFilter = CollisionFilter.Default;

        protected override void OnCreate()
        {
            RequireForUpdate<PhysicsWorldSingleton>();
        }

        protected override void OnUpdate()
        {
            _timer += SystemAPI.Time.DeltaTime;
            if (_timer < CheckInterval) return;
            _timer = 0f;

            if (Camera.main == null) return;

            Entity currentHitEntity = Entity.Null;

            // Выполняем рейкаст ТОЛЬКО если зажат ALT
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

                var input = new RaycastInput
                {
                    Start = ray.origin,
                    End = ray.origin + ray.direction * 1000f,
                    Filter = HoverFilter
                };

                if (physicsWorld.CastRay(input, out var hit))
                {
                    // Ищем именно объекты с UI-окнами
                    if (SystemAPI.HasComponent<HasPopupTag>(hit.Entity))
                    {
                        currentHitEntity = hit.Entity;
                    }
                }
            }

            // Логика Enter / Exit срабатывает и при отпускании ALT (currentHitEntity станет Entity.Null)
            if (currentHitEntity != _lastHoveredEntity)
            {
                var bridge = ClientBridge.World();

                // --- MOUSE EXIT ---
                if (_lastHoveredEntity != Entity.Null && EntityManager.Exists(_lastHoveredEntity))
                {
                    EntityManager.RemoveComponent<HoveredTag>(_lastHoveredEntity);
                    EntityBridge.TriggerMouseExit(new SingleEntity(_lastHoveredEntity, bridge));
                }

                // --- MOUSE ENTER ---
                if (currentHitEntity != Entity.Null)
                {
                    EntityManager.AddComponent<HoveredTag>(currentHitEntity);
                    EntityBridge.TriggerMouseEnter(new SingleEntity(currentHitEntity, bridge));
                }

                _lastHoveredEntity = currentHitEntity;
            }
        }
    }
}