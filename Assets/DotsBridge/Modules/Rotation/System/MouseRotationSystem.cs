using DotsBridge.Modules.Rotation;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DotsBridge.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.Default)]
    public partial class MouseRotationSystem : SystemBase
    {
        private Entity _activeEntity = Entity.Null;

        protected override void OnUpdate()
        {
            float dt = SystemAPI.Time.DeltaTime;

            // --- 1. ЗАХВАТ ---
            if (Input.GetMouseButtonDown(0))
            {
                var bridge = EntityBridge.InCurrentWorld();
                var hit = bridge.GetEntityUnderMouse<HoveredTag>();

                // Проверяем: есть ли конфиг И включен ли "рубильник" CanBeMouseRotated
                if (hit.Entity != Entity.Null && SystemAPI.IsComponentEnabled<CanBeMouseRotated>(hit.Entity))
                {
                    _activeEntity = hit.Entity;
                    EntityManager.SetComponentEnabled<IsCurrentlyDragging>(_activeEntity, true);
                }
            }

            // --- 2. ПРОЦЕСС ВРАЩЕНИЯ (Drag) ---
            if (Input.GetMouseButton(0) && _activeEntity != Entity.Null)
            {
                if (!EntityManager.Exists(_activeEntity)) { _activeEntity = Entity.Null; return; }

                float2 mouseDelta = new float2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

                var config = EntityManager.GetComponentData<MouseRotationConfig>(_activeEntity);
                var velocity = EntityManager.GetComponentData<MouseRotationVelocity>(_activeEntity);

                // Накапливаем скорость (Lerp для плавности самого движения мыши)
                velocity.Value = math.lerp(velocity.Value, mouseDelta * config.Sensitivity, 0.5f);
                EntityManager.SetComponentData(_activeEntity, velocity);

                ApplyRotation(_activeEntity, velocity.Value);
            }

            // --- 3. ИНЕРЦИЯ (Когда отпустили) ---
            if (Input.GetMouseButtonUp(0) && _activeEntity != Entity.Null)
            {
                if (EntityManager.Exists(_activeEntity))
                    EntityManager.SetComponentEnabled<IsCurrentlyDragging>(_activeEntity, false);
                _activeEntity = Entity.Null;
            }

            // Обработка инерции для ВСЕХ объектов, которые не зажаты прямо сейчас
            Entities
                .WithAll<CanBeMouseRotated, MouseRotationConfig>()
                .WithNone<IsCurrentlyDragging>()
                .ForEach((Entity e, ref MouseRotationVelocity vel, in MouseRotationConfig config) =>
                {
                    if (!config.UseInertia) { vel.Value = 0; return; }

                    if (math.length(vel.Value) > 0.01f)
                    {
                        ApplyRotation(e, vel.Value);
                        // Затухание
                        vel.Value *= config.Friction;
                    }
                    else
                    {
                        vel.Value = 0;
                    }
                }).WithStructuralChanges().Run();
        }

        private void ApplyRotation(Entity e, float2 vel)
        {
            var transform = EntityManager.GetComponentData<LocalTransform>(e);
            quaternion rotX = quaternion.AxisAngle(math.up(), -vel.x);
            quaternion rotY = quaternion.AxisAngle(math.right(), vel.y);

            transform.Rotation = math.mul(rotX, transform.Rotation);
            transform.Rotation = math.mul(transform.Rotation, rotY);
            EntityManager.SetComponentData(e, transform);
        }
    }
}