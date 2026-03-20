using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge.Character
{

    /// <summary>
    /// Система синхронизации камеры. Теперь она работает только с теми сущностями,
    /// у которых есть привязанная персональная камера.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class FirstPersonCameraSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Используем SystemAPI.ManagedAPI для доступа к классу CharacterCameraLink
            foreach (var (transform, settings, viewState, cameraLink) in SystemAPI.Query<
                             RefRO<LocalTransform>, 
                             RefRO<CharacterSettings>, 
                             RefRO<CharacterViewState>, 
                             CharacterCameraLink>()
                         .WithAll<ActiveCharacterTag>())
            {
                if (cameraLink.Camera == null) continue;

                // 1. Позиция: Позиция сущности (ноги) + высота глаз
                float3 eyePosition = transform.ValueRO.Position + new float3(0, settings.ValueRO.EyeHeight, 0);
                cameraLink.Camera.transform.position = eyePosition;

                // 2. Вращение: Yaw (от сущности) + Pitch (от наклона головы)
                quaternion pitchRotation = quaternion.Euler(math.radians(viewState.ValueRO.Pitch), 0, 0);
                cameraLink.Camera.transform.rotation = math.mul(transform.ValueRO.Rotation, pitchRotation);
            }
        }
    }
}