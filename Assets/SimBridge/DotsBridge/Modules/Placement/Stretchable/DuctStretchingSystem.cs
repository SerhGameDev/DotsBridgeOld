using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsBridge.Placement
{
    // Система работает СРАЗУ ПОСЛЕ GhostPlacementSystem
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GhostPlacementSystem))]
    public partial class DuctStretchingSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            foreach (var (transform, duct, postMatrix) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<StretchableDuct>, RefRW<PostTransformMatrix>>().WithAll<GhostTag>())
            {
                // Позиция, которую только что рассчитала GhostPlacementSystem (наша мышка)
                float3 cursorHitPos = transform.ValueRO.Position; 
                // Нормаль поверхности, которую мы получили из поворота
                float3 surfaceUp = math.mul(transform.ValueRO.Rotation, math.up());

                if (!duct.ValueRO.IsDrawing)
                {
                    // Режим ожидания: мы просто водим мышкой. Короб имеет минимальную длину.
                    duct.ValueRW.StartPoint = cursorHitPos;
                    duct.ValueRW.EndPoint = cursorHitPos;
                    
                    // Делаем его маленьким квадратиком
                    postMatrix.ValueRW.Value = float4x4.Scale(duct.ValueRO.Width, duct.ValueRO.Height, 0.05f);
                }
                else
                {
                    // Режим рисования: мы тянем короб!
                    duct.ValueRW.EndPoint = cursorHitPos;
                    
                    float3 start = duct.ValueRO.StartPoint;
                    float3 end = duct.ValueRO.EndPoint;
                    
                    float distance = math.distance(start, end);
                    // Защита от нулевой дистанции
                    if (distance < 0.01f) distance = 0.01f; 

                    float3 center = (start + end) * 0.5f;
                    float3 direction = math.normalize(end - start);

                    // 1. Ставим объект ровно посередине между точками
                    transform.ValueRW.Position = center;
                    
                    // 2. Поворачиваем так, чтобы он смотрел в сторону мышки, а "верх" был прижат к стене
                    transform.ValueRW.Rotation = quaternion.LookRotationSafe(direction, surfaceUp);
                    
                    // 3. Растягиваем ТОЛЬКО по оси Z
                    postMatrix.ValueRW.Value = float4x4.Scale(duct.ValueRO.Width, duct.ValueRO.Height, distance);
                }
            }
        }
    }
}