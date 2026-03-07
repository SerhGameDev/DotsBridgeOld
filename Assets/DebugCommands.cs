using DotsBridge;
using DotsBridge.Modules.Network;
using QFSW.QC;
using UnityEngine;

public static class DebugCommands
{
    // =========================================================
    // УНИВЕРСАЛЬНЫЙ ВЫЗОВ (ДЛЯ ВСЕХ КОМАНД)
    // =========================================================

    [Command("exec", "Выполнить любую зарегистрированную команду DotsBridge по имени")]
    public static void ExecuteBridgeCommand(string commandName)
    {
        // Это обратится к нашему словарю _commandRegistry
        EntityBridge.Execute(commandName);
    }

    // =========================================================
    // БЫСТРЫЕ КОМАНДЫ (SHORTCUTS)
    // =========================================================

    [Command("jump-s", "Серверный прыжок")]
    public static void JumpServer() => EntityBridge.Execute("PlayerJumpServer");

    [Command("jump-c", "Клиентский прыжок")]
    public static void JumpClient() => EntityBridge.Execute("PlayerJumpClient");

    [Command("heal", "Вылечить игрока")]
    public static void Heal() => EntityBridge.Execute("PlayerHeal");

    // =========================================================
    // ДИНАМИЧЕСКИЙ СПАВН ЧЕРЕЗ КОНСОЛЬ
    // =========================================================

    [Command("spawn-cube", "Заспавнить куб с указанным ID и позицией")]
    public static void SpawnCube(string id, float x, float y, float z)
    {
        EntityBridge.BeginSpawnForServer("Cub_Server")
            .SetId(id)
            .SetPosition(new Vector3(x, y, z))
            .Spawn()
            .Execute();

        Debug.Log($"[Console] Запрос на спавн {id} отправлен на {x}, {y}, {z}");
    }

    [Command("spawn-cube-server-zero", "Заспавнить куб на сервере")]
    public static void SpawnCubeServer()
    {
        EntityBridge.BeginSpawnForServer("Cub_Server")
            .SetId("Cub_Server")
            .SetPosition(new Vector3(0, 0, 0))
            .SpawnAsync();

        Debug.Log($"[Console] Запрос на спавн Cub_Server");
    }
    [Command("spawn-cube-server", "Заспавнить куб на сервере")]
    public static void SpawnCubeServer(float x, float y, float z)
    {
        EntityBridge.BeginSpawnForServer("Cub_Server")
            .SetId("Cub_Server")
            .SetPosition(new Vector3(x, y, z))
            .Spawn()
            .Execute();

        Debug.Log($"[Console] Запрос на спавн Cub_Server отправлен на {x}, {y}, {z}");
    }

    [Command("spawn-cube-client", "Заспавнить куб на сервере")]
    public static void SpawnCubeClient(float x, float y, float z)
    {
        EntityBridge.BeginSpawnForClient("Cub_Client")
            .SetId("Cub_Client")
            .SetPosition(new Vector3(x, y, z))
            .Spawn()
            .Execute();

        Debug.Log($"[Console] Запрос на спавн Cub_Client отправлен на {x}, {y}, {z}");
    }

    // =========================================================
    // СЕТЕВЫЕ КОМАНДЫ
    // =========================================================

    [Command("connect", "Подключиться к серверу")]
    public static void Connect()
    {
        // Используем Instance нашего менеджера
        DotsNetworkManager.Instance.ConnectToServer();
    }

    [Command("start-server", "Запустить сервер")]
    public static void StartServer()
    {
        DotsNetworkManager.Instance.StartServer();
    }

    [Command("get-count", "Проверить количество сущностей по ID")]
    public static void GetCount(string id)
    {
        var batch = EntityBridge.GetForServer(id);
        batch.LogCount(customMessage: $"Поиск для ID: {id}");
    }
}