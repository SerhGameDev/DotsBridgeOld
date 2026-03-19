using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using DotsBridge;
using DotsBridge.Build;
using EntityBridge = DotsBridge.EntityBridge;

public class RoomDebugger : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Имя префаба стены в твоем PrefabContainer")]
    public string WallPrefabName = "WallPrefab";
    public float CellSize = 1f;
    
    [Header("Статус (Только для чтения)")]
    public int DetectedRooms = 0;
    public float LastArea = 0f;

    private BridgeWorld _world;
    private bool _wallsBuilt = false;

    private void Start()
    {
        // Ждем немного, чтобы DotsBridgeBootstrapper и PrefabContainer успели инициализироваться
        StartCoroutine(DelayedBuild());
    }

    private IEnumerator DelayedBuild()
    {
        yield return new WaitForSeconds(1);

        _world = EntityBridge.InCurrentWorld();
        
        if (_world == null)
        {
            Debug.LogError("[RoomDebugger] BridgeWorld не найден! Проверь DotsBridgeBootstrapper.");
            yield break;
        }

        _world.InitializeGrid(CellSize);

        // Строим большую комнату 10х10 (чтобы было хорошо видно на сцене)
        _world.BuildWall(WallPrefabName, new float3(0, 0, 0), new float3(10, 0, 0)).Execute();
        _world.BuildWall(WallPrefabName, new float3(10, 0, 0), new float3(10, 0, 10)).Execute();
        _world.BuildWall(WallPrefabName, new float3(10, 0, 10), new float3(0, 0, 10)).Execute();
        _world.BuildWall(WallPrefabName, new float3(0, 0, 10), new float3(0, 0, 0)).Execute();

        // Триггерим пересчет
        _world.Manager.CreateEntity(typeof(TopologyChangedTag));
        
        _wallsBuilt = true;
        Debug.Log("[RoomDebugger] Команды на постройку 4 стен отправлены!");
    }
    

    private void Update()
    {
        if (!_wallsBuilt || _world == null) return;

        var roomQuery = _world.Manager.CreateEntityQuery(typeof(RoomTag), typeof(RoomData));
        DetectedRooms = roomQuery.CalculateEntityCount();

        if (DetectedRooms > 0)
        {
            // Безопасно берем первую комнату из массива, вместо GetSingleton()
            using var rooms = roomQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            var roomData = _world.Manager.GetComponentData<RoomData>(rooms[0]);
            LastArea = roomData.Area;
        }
    }
    // Рисуем отладочную графику прямо в редакторе (вкладка Scene)
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || _world == null) return;

        // Рисуем все стены, которые реально существуют в ECS
        var wallQuery = _world.Manager.CreateEntityQuery(typeof(WallComponent));
        var walls = wallQuery.ToComponentDataArray<WallComponent>(Unity.Collections.Allocator.Temp);

        Gizmos.color = Color.yellow;
        foreach (var wall in walls)
        {
            Gizmos.DrawLine(wall.Start, wall.End);
            Gizmos.DrawSphere(wall.Start, 0.2f);
            Gizmos.DrawSphere(wall.End, 0.2f);
        }
        walls.Dispose();

        // Если комната найдена, рисуем зеленую зону
        if (DetectedRooms > 0)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(new Vector3(5, 0.5f, 5), new Vector3(10, 1, 10)); // Центр нашей комнаты 10х10
        }
    }
}