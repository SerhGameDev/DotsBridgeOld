using DotsBridge.Modules.Movement;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Задает статичное направление движения (Vector3).
        /// </summary>
        public static DotsCommand Move(this DotsCommand cmd, Vector3 direction)
        {
            // Просто продолжаем цепочку команд!
            return cmd.SetData(new MoveTransformDirection { Value = direction });
        }

        /// <summary>
        /// Задает статичное направление движения (float3 для DOTS).
        /// </summary>
        public static DotsCommand Move(this DotsCommand cmd, float3 direction)
        {
            return cmd.SetData(new MoveTransformDirection { Value = direction });
        }

        /// <summary>
        /// Задает динамическое направление (вычисляется каждый кадр при вызове Execute).
        /// Идеально для WASD инпута в Top-Down шутерах!
        /// </summary>
        public static DotsCommand Move(this DotsCommand cmd, Func<float3> directionProvider)
        {
            return cmd.Do(batch =>
            {
                if (batch.Entities.IsEmpty) return;

                // Читаем инпут строго в момент выполнения команды
                float3 dir = directionProvider.Invoke();
                var em = batch.State.Manager;

                foreach (var entity in batch.Entities)
                {
                    if (em.HasComponent<MoveTransformDirection>(entity))
                    {
                        em.SetComponentData(entity, new MoveTransformDirection { Value = dir });
                    }
                }
            });
        }
    }
}