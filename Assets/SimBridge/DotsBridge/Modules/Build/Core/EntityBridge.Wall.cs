using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsBridge.Build
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Создает команду на строительство стены между двумя точками.
        /// Точки автоматически притягиваются к узлам сетки.
        /// </summary>
        public static DotsCommand BuildWall(this BridgeWorld world, string prefabName, float3 start, float3 end, float thickness = 0.2f)
        {
            float cellSize = world.GetCellSize();
            var prefab =  world.GetPrefab(prefabName);

            float3 snappedStart = math.round(start / cellSize) * cellSize;
            float3 snappedEnd = math.round(end / cellSize) * cellSize;

            var command = new DotsCommand($"BuildWall_{prefabName}");

            command.SetTargetResolver(() =>
            {
                var batch = new ListEntity(world);
                
                // 1. Создаем сущность стены
                var wallEntity = world.Manager.Instantiate(prefab);
                batch.Entities.Add(wallEntity);

                // 2. Добавляем данные стены
                world.Manager.AddComponentData(wallEntity, new WallComponent 
                { 
                    Start = snappedStart, 
                    End = snappedEnd, 
                    Thickness = thickness 
                });
                world.Manager.AddComponent<WallTag>(wallEntity);
                
                // 3. Добавляем буфер для будущих соединений
                world.Manager.AddBuffer<ConnectionElement>(wallEntity);

                // 4. Настройка визуального трансформа (Позиция в центре + поворот вдоль линии)
                float3 center = (snappedStart + snappedEnd) * 0.5f;
                float3 direction = math.normalize(snappedEnd - snappedStart);
                quaternion rotation = quaternion.LookRotationSafe(direction, math.up());
                
                // Длина стены для масштабирования (Scale.z или Scale.x зависит от вашего префаба)
                float length = math.distance(snappedStart, snappedEnd);

                world.Manager.SetComponentData(wallEntity, LocalTransform.FromPositionRotationScale(
                    center, 
                    rotation, 
                    1f // Масштаб обычно настраивается через процедурный меш или шейдер, но можно и через Transform
                ));

                // Добавляем в Spatial Hash по обоим концам для быстрого поиска соединений
                var gridStart = new GridPosition(snappedStart, cellSize);
                var gridEnd = new GridPosition(snappedEnd, cellSize);
                
                // Мы вешаем только один GridPosition для основной регистрации, 
                // но система SpatialGridSystem может быть расширена для регистрации двух точек.
                world.Manager.AddComponentData(wallEntity, gridStart);

                return batch;
            }, true);

            return command;
        }
    }
}