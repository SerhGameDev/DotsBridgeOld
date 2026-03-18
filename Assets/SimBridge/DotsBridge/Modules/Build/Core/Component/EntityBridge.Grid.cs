using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using DotsBridge.Build;

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
        /// Подготавливает SpawnerBuilder для пола с автоматическим снаппингом к сетке.
        /// Возвращает билдер для дальнейшего вызова .SpawnAsync() или .Spawn().
        /// </summary>
        public static SpawnerBuilder BuildFloor(this BridgeWorld world, string prefabName, float3 worldPosition)
        {
            float cellSize = world.GetCellSize();
            
            // Вычисляем дискретную позицию и переводим обратно в ровные мировые координаты
            var gridPos = GridPosition.FromWorldPosition(worldPosition, cellSize);
            var snappedPos = gridPos.ToWorldPosition(cellSize);

            // Используем твой внутренний метод поиска префаба
            var prefab = world.GetPrefab(prefabName);

            // Возвращаем настроенный билдер
            return new SpawnerBuilder(world, prefab)
                .SetPosition(snappedPos);
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