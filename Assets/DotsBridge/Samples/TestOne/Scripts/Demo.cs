using DotsBridge;
using DotsBridge.Modules.Movement;
using Unity.Mathematics;
using UnityEngine;

public class Demo : MonoBehaviour
{
    private DotsCommand _moveCommand;
    private DotsCommand _shootCommand;
    private bool _isSpawned = false;

    private void Start()
    {
        // 1. СПАВН (Инициализация на сервере)
        // Имитируем старт матча. Сервер создает игрока.
        EntityBridge.BeginSpawn("PlayerPrefab")
            .SetPosition(new Vector3(0, 5, 0))
            .Spawn("MyPlayer") // Регистрируем под ID "MyPlayer"
            .SetData(new MoveTransformSpeed { Value = 5f })
            .ServerExecute(); // Терминальный метод спавнера

        // 2. СБОРКА КОМАНД (Кэшируем логику, но ничего не выполняем)
        // Команда движения: будет брать инпут каждый раз при вызове
        _moveCommand = EntityBridge.Command("PlayerMovement")
            .GetById("MyPlayer")
            .Move(GetTopDownInput);

        // Команда стрельбы (просто для примера)
        _shootCommand = EntityBridge
            .GetById("MyPlayer")
            .Do(batch => Debug.Log("[Сервер] Игрок выстрелил!"));

        _isSpawned = true;
    }

    private void Update()
    {
        if (!_isSpawned) return;

        // 3. ВЫПОЛНЕНИЕ (Мутации)
        // Отправляем команду движения на сервер каждый кадр
        _moveCommand.ServerExecute();

        // Отправляем команду стрельбы только по клику
        if (Input.GetMouseButtonDown(0))
        {
            _shootCommand.ServerExecute();
        }

        // 4. ЧТЕНИЕ (Query - безопасно для UI)
        // По нажатию на 'R' просто читаем данные скорости, не создавая мутаций
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Обращаемся к Read-Only API
            var batch = EntityBridge.QueryById("MyPlayer", EntityBridge.ServerState);

            // Получаем данные
            var speedData = batch.GetFirstData<MoveTransformSpeed>();
            Debug.Log($"<color=cyan>[UI] Текущая скорость игрока: {speedData.Value}</color>");

            // Обязательно очищаем батч, так как он использует NativeList
            batch.Dispose();
        }
    }

    // Вспомогательный метод для динамического инпута (Top-Down X/Z)
    private float3 GetTopDownInput()
    {
        return new float3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }
}