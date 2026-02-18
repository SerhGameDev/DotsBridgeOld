using DotsBridge.Movement;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge
{
    public static partial class Dots
    {
        // -----------------------------------------------------------------------------------
        // GROUP MOVEMENT API (NativeArray & NativeList)
        // -----------------------------------------------------------------------------------

        /// <summary>
        /// Отправить группу сущностей в одну точку.
        /// </summary>
        public static void MoveTo(NativeArray<Entity> entities, Vector3 destination, float speed = 5f, bool rotate = true)
        {
            if (entities.Length == 0) return;

            PrepareMoveDataBatch(entities, speed, rotate);

            Manager.RemoveComponent<MoveDirection>(entities);

            Manager.AddComponent<MoveTarget>(entities);
            Manager.AddComponent<StopDistance>(entities);

            var target = new MoveTarget { Value = destination };
            var stopDist = new StopDistance { Value = 0.1f };

            for (int i = 0; i < entities.Length; i++)
            {
                Manager.SetComponentData(entities[i], target);
                Manager.SetComponentData(entities[i], stopDist);

                EnableMovementEngine(entities[i]);
            }
        }

        public static void MoveTo(NativeList<Entity> entities, Vector3 destination, float speed = 5f, bool rotate = true)
        {
            MoveTo(entities.AsArray(), destination, speed, rotate);
        }


        /// <summary>
        /// Отправить группу сущностей в одном направлении.
        /// </summary>
        public static void MoveDir(NativeArray<Entity> entities, Vector3 direction, float speed = 5f, bool rotate = true)
        {
            if (entities.Length == 0) return;

            PrepareMoveDataBatch(entities, speed, rotate);

            Manager.RemoveComponent<MoveTarget>(entities);
            Manager.RemoveComponent<StopDistance>(entities);

            Manager.AddComponent<MoveDirection>(entities);

            var dirData = new MoveDirection { Value = math.normalize(direction) };

            for (int i = 0; i < entities.Length; i++)
            {
                Manager.SetComponentData(entities[i], dirData);
                EnableMovementEngine(entities[i]);
            }
        }

        public static void MoveDir(NativeList<Entity> entities, Vector3 direction, float speed = 5f, bool rotate = true)
        {
            MoveDir(entities.AsArray(), direction, speed, rotate);
        }


        /// <summary>
        /// Остановить группу сущностей.
        /// </summary>
        public static void Stop(NativeArray<Entity> entities)
        {
            if (entities.Length == 0) return;

            // Выключаем теги движения (это не структурное изменение, просто смена бита)
            // К сожалению, SetComponentEnabled нет для NativeArray, приходится в цикле
            for (int i = 0; i < entities.Length; i++)
            {
                SetEnabled<MoveWithPhysicsTag>(entities[i], false);
                SetEnabled<MoveWithTransformTag>(entities[i], false);

                // Сброс физики
                if (Has<Unity.Physics.PhysicsVelocity>(entities[i]))
                {
                    var vel = Get<Unity.Physics.PhysicsVelocity>(entities[i]);
                    vel.Linear = float3.zero;
                    Set(entities[i], vel);
                }
            }
        }

        public static void Stop(NativeList<Entity> entities)
        {
            Stop(entities.AsArray());
        }


        private static void PrepareMoveDataBatch(NativeArray<Entity> entities, float speed, bool rotate)
        {
            Manager.AddComponent<MoveSpeed>(entities);

            var speedData = new MoveSpeed { Value = speed };

            if (rotate)
            {
                Manager.AddComponent<RotationSpeed>(entities);
                Manager.AddComponent<RotateToMovementTag>(entities); 

                var rotSpeed = new RotationSpeed { Value = 10f };

                for (int i = 0; i < entities.Length; i++)
                {
                    Manager.SetComponentData(entities[i], speedData);
                    Manager.SetComponentData(entities[i], rotSpeed);
                    Manager.SetComponentEnabled<RotateToMovementTag>(entities[i], true);
                }
            }
            else
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    Manager.SetComponentData(entities[i], speedData);
                    if (Manager.HasComponent<RotateToMovementTag>(entities[i]))
                        Manager.SetComponentEnabled<RotateToMovementTag>(entities[i], false);
                }
            }
        }

        private static void EnableMovementEngine(Entity entity)
        {
            if (Has<Unity.Physics.PhysicsVelocity>(entity))
            {
                if (!Has<MoveWithPhysicsTag>(entity)) Manager.AddComponent<MoveWithPhysicsTag>(entity);
                SetEnabled<MoveWithPhysicsTag>(entity, true);

                SetEnabled<MoveWithTransformTag>(entity, false);
            }
            else
            {
                if (!Has<MoveWithTransformTag>(entity)) Manager.AddComponent<MoveWithTransformTag>(entity);
                SetEnabled<MoveWithTransformTag>(entity, true);

                SetEnabled<MoveWithPhysicsTag>(entity, false);
            }
        }

        private static void SetEnabled<T>(Entity entity, bool state) where T : IComponentData, IEnableableComponent
        {
            if (Manager.HasComponent<T>(entity)) Manager.SetComponentEnabled<T>(entity, state);
        }
    }
}