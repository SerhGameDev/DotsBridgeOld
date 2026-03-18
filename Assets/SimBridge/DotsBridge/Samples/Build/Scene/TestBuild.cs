using DotsBridge;
using DotsBridge.Build;
using UnityEngine;

public class TestBuild : MonoBehaviour
{
    private void Start()
    {
        var clientWorld = EntityBridge.InClientWorld();

// Инициализируем (один раз)
        clientWorld.InitializeGrid(2.0f);

// Размещаем пол по клику мыши (асинхронно через твой SpawnRequest)
        var mousePos = clientWorld.GetMousePoint<TerrainTag>();
        clientWorld.BuildFloor("Floor_Wood", mousePos).SpawnAsync();

// Размещаем с кастомным ID и синхронно через команду
        clientWorld.BuildFloor("Floor_Tile", 5, 5)
            .SetId("Tile_01")
            .Spawn("BuildCommand_Tile_01");
    }

}

internal struct TerrainTag
{
}
