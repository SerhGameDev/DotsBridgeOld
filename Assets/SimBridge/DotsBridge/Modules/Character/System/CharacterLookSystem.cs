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
            foreach (var (transform, input, settings, viewState) in SystemAPI.Query<
                             RefRW<LocalTransform>, 
                             RefRO<CharacterControlInput>, 
                             RefRO<CharacterSettings>, 
                             RefRW<CharacterViewState>>()
                         .WithAll<ActiveCharacterTag>())
            {
                // 1. Вращение тела влево/вправо (Yaw)
                float yawDelta = input.ValueRO.LookInput.x * settings.ValueRO.LookSpeed;
                
                // Создаем поворот ТОЛЬКО по Y
                quaternion currentRotation = transform.ValueRO.Rotation;
                quaternion extraRotation = quaternion.Euler(0, math.radians(yawDelta), 0);
                
                // Комбинируем и ВАЖНО: избавляемся от наклонов по X и Z
                // Мы берем новый поворот по Y, но жестко задаем Up-вектор
                quaternion combined = math.mul(currentRotation, extraRotation);
                float3 forward = math.mul(combined, math.forward());
                forward.y = 0; // Направляем взгляд строго в горизонт для расчета тела
                
                // Пересобираем вращение, чтобы оно было строго вертикальным
                transform.ValueRW.Rotation = quaternion.LookRotationSafe(forward, math.up());

                // 2. Вращение головы (Pitch) - остается как было
                float pitchDelta = -input.ValueRO.LookInput.y * settings.ValueRO.LookSpeed;
                viewState.ValueRW.Pitch = math.clamp(viewState.ValueRO.Pitch + pitchDelta, -85f, 85f);
            }
        }
    }
}