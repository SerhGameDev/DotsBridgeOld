using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using DotsBridge.Build;
using Unity.Transforms;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Инициализация сетки для конкретного мира (Server или Client).
        /// </summary>
        public static void InitializeGrid(this BridgeWorld world, float cellSize = 1f)
        {
            var gridEntity = SingleEntity.GetOrCreateSingleton<GridSettings>(world);
            gridEntity.SetComponent(new GridSettings { CellSize = cellSize });
            Debug.Log($"[DotsBridge] Сетка инициализирована в мире {world.World.Name}. Размер ячейки: {cellSize}");
        }

        /// <summary>
        /// Безопасное получение размера ячейки сетки для текущего мира.
        /// </summary>
        public static float GetCellSize(this BridgeWorld world)
        {
            var query = world.Manager.CreateEntityQuery(typeof(GridSettings));
            return query.IsEmptyIgnoreFilter ? 1f : query.GetSingleton<GridSettings>().CellSize;
        }

        /// <summary>
        /// Создает команду на строительство пола. 
        /// Возвращает DotsCommand, которую можно дополнить и вызвать через .Execute()
        /// </summary>
        public static DotsCommand BuildFloor(this BridgeWorld world, string prefabName, float3 position)
        {
            float cellSize = world.GetCellSize();
            var prefab = world.GetPrefab(prefabName);
            
            // Рассчитываем данные сетки заранее
            var gridData = new GridPosition(position, cellSize);
            // Снаппим позицию для трансформа (если нужно строгое выравнивание)
            float3 snappedPos = new float3(gridData.GridIndex) * cellSize;

            // Создаем команду
            var command = new DotsCommand($"BuildFloor_{prefabName}");

            // Настраиваем логику создания внутри команды
            command.SetTargetResolver(() =>
            {
                // Используем твой ListEntity для создания
                var batch = new ListEntity(world);
                batch.Instantiate(prefab, 1);
                
                // Добавляем базовые компоненты строительства
                batch.AddComponent(gridData);
                batch.AddComponent<FloorTag>();
                
                // Устанавливаем трансформ
                if (world.Manager.HasComponent<LocalTransform>(batch.Entities[0]))
                {
                    batch.TrySetComponent(LocalTransform.FromPosition(snappedPos));
                }

                return batch;
            }, true);

            return command;
        }
    

        /// <summary>
        /// Перегрузка для точечного строительства по координатам сетки.
        /// </summary>
        public static SpawnerBuilder BuildFloor(this BridgeWorld world, string prefabName, int x, int z)
        {
            float cellSize = world.GetCellSize();
            var snappedPos = new float3(x * cellSize, 0, z * cellSize);
            
            var prefab = world.GetPrefab(prefabName);
            return new SpawnerBuilder(world, prefab)
                .SetPosition(snappedPos);
        }
    }
}