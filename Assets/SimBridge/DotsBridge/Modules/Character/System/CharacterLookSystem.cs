using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsBridge.Character
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class CharacterLookSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (transform, input, settings, viewState) in SystemAPI.Query<
                             RefRW<LocalTransform>, 
                             RefRO<CharacterControlInput>, 
                             RefRO<CharacterSettings>, 
                             RefRW<CharacterViewState>>()
                         .WithAll<ActiveCharacterTag>())
            {
                // Если мышь не двигалась, пропускаем вычисления
                if (input.ValueRO.LookInput.x == 0 && input.ValueRO.LookInput.y == 0) continue;
                
                // 1. Вращение тела влево/вправо (Yaw)
                // Умножаем на deltaTime для независимости от FPS
                float yawDelta = input.ValueRO.LookInput.x * settings.ValueRO.LookSpeed * deltaTime;
                transform.ValueRW = transform.ValueRW.RotateY(yawDelta);

                // 2. Вращение головы вверх/вниз (Pitch)
                // Инвертируем Y, чтобы мышь вверх поднимала взгляд
                float pitchDelta = -input.ValueRO.LookInput.y * settings.ValueRO.LookSpeed * deltaTime;
                float newPitch = viewState.ValueRO.Pitch + pitchDelta;

                // Ограничиваем угол, чтобы игрок не свернул себе шею (например, от -85 до 85 градусов)
                viewState.ValueRW.Pitch = math.clamp(newPitch, -85f, 85f);
            }
        }
    }
}