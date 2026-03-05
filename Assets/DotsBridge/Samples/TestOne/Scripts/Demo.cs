using DotsBridge;
using DotsBridge.Modules.Movement;
using DotsBridge.Modules.Network;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Demo : MonoBehaviour
{
    [SerializeField] private int Count = 10;

    private bool _isServerStart;
    private DotsCommand _moveCommand;

    private void OnEnable()
    {
        // Теперь подписываемся на старт СЕРВЕРА
        DotsNetworkManager.OnServerStarted += OnServerSuccess;
    }

    private void OnDisable()
    {
        DotsNetworkManager.OnServerStarted -= OnServerSuccess;
    }

    private void OnServerSuccess()
    {
        Debug.Log("<color=yellow>Сервер запущен! Начинаем спавн...</color>");
        StartCoroutine(ServerSpawnCoroutine());
    }

    public IEnumerator ServerSpawnCoroutine()
    {
        yield return new WaitForSeconds(1);

        // 1. СПАВНИМ ТОЛЬКО НА СЕРВЕРЕ
        EntityBridge
            .InServerWorld()
            .BeginSpawn("Cub") // Имя твоего префаба
            .SetCount(Count)
            .SetPosition(new Vector3(0, 5, 0))
            .Spawn("EnemyWave1")
            .SetData(new MoveTransformSpeed { Value = 5 })
            .Do(batch => Debug.Log($"[Сервер] Волна появилась! Юнитов: {batch.Entities.Length}"))
            .Execute();


        yield return new WaitForSeconds(1);
        _isServerStart = true;
    }
}