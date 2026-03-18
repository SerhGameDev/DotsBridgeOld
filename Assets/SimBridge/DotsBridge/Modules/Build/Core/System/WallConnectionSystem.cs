using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Build
{
    [BurstCompile]
    [UpdateAfter(typeof(SpatialGridSystem))]
    public partial struct WallConnectionSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Получаем данные пространственной сетки
            if (!SystemAPI.TryGetSingleton<SpatialGridData>(out var gridData)) return;

            // Запускаем Job для обновления связей
            var job = new UpdateWallConnectionsJob
            {
                GridMap = gridData.Map
            };
            
            job.ScheduleParallel();
        }

        [BurstCompile]
        private partial struct UpdateWallConnectionsJob : IJobEntity
        {
            [ReadOnly] public NativeParallelMultiHashMap<int3, Entity> GridMap;

            // Мы просим доступ на запись (ref) к буферу связей
            public void Execute(Entity entity, ref DynamicBuffer<ConnectionElement> wallBuffer, in WallComponent wallData)
            {
                wallBuffer.Clear();

                // Проверяем оба конца стены
                FindConnections(entity, wallData.Start, ref wallBuffer);
                FindConnections(entity, wallData.End, ref wallBuffer);
            }

            private void FindConnections(Entity self, float3 point, ref DynamicBuffer<ConnectionElement> buffer)
            {
                // Квантование мировых координат в индекс сетки (как в GridPosition)
                // cellSize берем 1.0f для примера или передаем из GridSettings
                int3 gridIdx = new int3(math.round(point)); 

                if (GridMap.TryGetFirstValue(gridIdx, out Entity other, out var it))
                {
                    do 
                    {
                        // Не соединяем стену саму с собой и проверяем на дубликаты
                        if (other != self && !AlreadyConnected(buffer, other))
                        {
                            buffer.Add(new ConnectionElement { ConnectedWall = other });
                        }
                    } while (GridMap.TryGetNextValue(out other, ref it));
                }
            }

            private bool AlreadyConnected(DynamicBuffer<ConnectionElement> buffer, Entity e)
            {
                for (int i = 0; i < buffer.Length; i++)
                    if (buffer[i].ConnectedWall == e) return true;
                return false;
            }
        }
    }
}