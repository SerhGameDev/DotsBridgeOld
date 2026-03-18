using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using DotsBridge.Build;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Возвращает все сущности, которые находятся в указанной ячейке сетки.
        /// Не забудьте вызвать Dispose() или обернуть в using, так как возвращается новый ListEntity.
        /// </summary>
        public static ListEntity GetEntitiesAt(this BridgeWorld world, int3 gridPos)
        {
            var query = world.Manager.CreateEntityQuery(typeof(SpatialGridData));
            
            if (query.IsEmptyIgnoreFilter)
                return new ListEntity(world, Allocator.Temp);

            var gridData = query.GetSingleton<SpatialGridData>();
            var batch = new ListEntity(world, Allocator.Temp);

            // Ищем все сущности по ключу (координате)
            if (gridData.Map.TryGetFirstValue(gridPos, out Entity entity, out var iterator))
            {
                batch.Entities.Add(entity);
                
                // Проходимся по всем остальным сущностям в этой же ячейке
                while (gridData.Map.TryGetNextValue(out entity, ref iterator))
                {
                    batch.Entities.Add(entity);
                }
            }

            return batch;
        }

        /// <summary>
        /// Быстрая проверка: есть ли в этой ячейке сущность с конкретным компонентом (например, FloorTag)?
        /// </summary>
        public static bool HasEntityWith<T>(this BridgeWorld world, int3 gridPos) where T : unmanaged, IComponentData
        {
            using var entitiesAtPos = world.GetEntitiesAt(gridPos);
            
            for (int i = 0; i < entitiesAtPos.Count; i++)
            {
                if (world.Manager.HasComponent<T>(entitiesAtPos.Entities[i]))
                {
                    return true; // Нашли нужный объект
                }
            }
            
            return false;
        }
    }
}