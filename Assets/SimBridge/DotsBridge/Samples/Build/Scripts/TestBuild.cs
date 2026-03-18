using DotsBridge;
using DotsBridge.Build;
using Unity.Mathematics;
using UnityEngine;

public class TestBuild : MonoBehaviour
{
    private void Start()
    {
        var clientWorld = EntityBridge.InCurrentWorld();

// Инициализируем (один раз)
        clientWorld.InitializeGrid(2.0f);

        var targetPos = new int3(5, 0, 5);

// Проверяем, есть ли уже пол в этой клетке
        if (!clientWorld.HasEntityWith<FloorTag>(targetPos))
        {
            clientWorld.BuildFloor("Floor_Tile", targetPos.x, targetPos.z)
                .Spawn().Execute();
        }
    }

}
