using DotsBridge.Modules.Rotation;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace DotsBridge.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.Default)]
    public partial class MouseHoverSystem : SystemBase
    {
        private Entity _lastHoveredEntity = Entity.Null;

        public float CheckInterval = 0.05f;
        private float _timer = 0f;

        protected override void OnCreate()
        {
            // Система ждет, пока появится физика
            RequireForUpdate<PhysicsWorldSingleton>();
        }

        protected override void OnUpdate()
        {
            // Ограничитель частоты (Timer)
            _timer += SystemAPI.Time.DeltaTime;
            if (_timer < CheckInterval) return;
            _timer = 0f;

            // Защита от запуска на сервере без камеры
            if (Camera.main == null) return;

            // 1. Создаем луч
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            var input = new RaycastInput
            {
                Start = ray.origin,
                End = ray.origin + ray.direction * 1000f,
                Filter = CollisionFilter.Default
            };

            Entity currentHitEntity = Entity.Null;

            // 2. Пускаем луч через Unity Physics
            if (physicsWorld.CastRay(input, out var hit))
            {
                // Если попали в объект с InteractableTag, запоминаем его
                if (SystemAPI.HasComponent<InteractableTag>(hit.Entity))
                {
                    currentHitEntity = hit.Entity;
                }
            }

            // 3. Логика Enter / Exit
            if (currentHitEntity != _lastHoveredEntity)
            {
                var bridge = EntityBridge.GetOrCreateBridge(World);

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