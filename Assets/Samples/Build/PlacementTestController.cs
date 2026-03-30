using DotsBridge;
using DotsBridge.Interaction;
using DotsBridge.Placement;
using SimElectric;
using SimOil;
using UnityEngine;

public class PlacementTestController : MonoBehaviour
{
    [SerializeField] private float _step = 0.1f;
    [SerializeField] private string _prefabNameToSpawn = "RelayCube"; // Имя префаба из вашего реестра

    private bool _wasValid = true;

    private void OnEnable()
    {
        EntityBridge.OnPlacementSuccess += HandlePlacementSuccess;
        EntityBridge.OnPlacementFailed += HandlePlacementFailed;
        EntityBridge.OnDuctDrawingStarted += HandleDuctDrawing;
    }

    private void OnDisable()
    {
        EntityBridge.OnPlacementSuccess -= HandlePlacementSuccess;
        EntityBridge.OnPlacementFailed -= HandlePlacementFailed;
        EntityBridge.OnDuctDrawingStarted -= HandleDuctDrawing;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            // 1. ВАЖНО: Мы берем именно Клиентский мир! 
            // InCurrentWorld может вернуть Серверный мир, если мы играем за Хоста, 
            // но для UI всегда правильнее и безопаснее читать из мира Клиента (призраков).
            var clientBridge = ClientBridge.World();
            
            if (clientBridge == null)
            {
                Debug.LogWarning("Клиентский мир еще не создан или мы не подключены.");
                return;
            }

            // 2. Ищем все сущности с жидкостью.
            // ВАЖНО: Так как твой метод FindWithComponent создает NativeList (Temp), 
            // мы обязательно используем ключевое слово 'using', чтобы память очистилась в конце кадра!
            using var batch = clientBridge.FindWithComponent<FluidMixture>();

            if (batch.Count == 0)
            {
                Debug.Log("На клиенте пока нет узлов FluidMixture (Призраки еще не прилетели).");
                return;
            }

            Debug.Log($"=== ДАННЫЕ С СЕРВЕРА (Найдено узлов: {batch.Count}) ===");

            var em = clientBridge.Manager;

            // 3. Перебираем все найденные сущности в батче
            for (int i = 0; i < batch.Count; i++)
            {
                var entity = batch.Entities[i];
                    
                // Читаем синхронизированный компонент
                var mixture = em.GetComponentData<FluidMixture>(entity);

                // Выводим в консоль
                Debug.Log($"Узел [Entity {entity.Index}]: " +
                          $"Масса = {mixture.TotalMass:F1} кг | " +
                          $"Давление = {mixture.Pressure:F2} МПа | " +
                          $"Температура = {mixture.Temperature:F1}°C");
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ClientBridge.World().BeginSpawn(_prefabNameToSpawn).Spawn().BeginPlacement(_step);
            ClientBridge.World().ToggleCurrentGridSnap();
            Debug.Log("[SimBridge Editor] Выбран компонент. ЛКМ - разместить, G - вкл/выкл сетку.");
        }
        if (Input.GetMouseButtonDown(1))
        {
            ClientBridge.World().GetEntityUnderMouse<HingeTrigger>().TriggerHinge();
            ClientBridge.World().GetEntityUnderMouse<PushButtonTrigger>().TriggerButton();
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            bool isSnapActive = ClientBridge.World().ToggleCurrentGridSnap();
            Debug.Log($"[SimBridge Editor] Привязка к сетке: {(isSnapActive ? "ВКЛ" : "ВЫКЛ")}");
        }

        // --- 3. UI-ИНДИКАЦИЯ ---
        if (ClientBridge.World().HasActiveGhost())
        {
            bool isValid = EntityBridge.IsCurrentPlacementValid();
            if (isValid != _wasValid)
            {
                _wasValid = isValid;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (ClientBridge.World().HasActiveGhost())
            {
                EntityBridge.CompleteCurrentPlacement();
            }
            else 
            {
                ClientBridge.World().GetEntityUnderMouse<PlaceableTag>().BeginPlacement(_step);
            }
        }
    }

    // --- ОБРАБОТЧИКИ СОБЫТИЙ ---
    private void HandlePlacementSuccess(SingleEntity entity) => Debug.Log("[SimBridge Editor] Успех: Объект зафиксирован!");
    private void HandlePlacementFailed(SingleEntity entity) => Debug.LogWarning("[SimBridge Editor] Ошибка: Место занято или позиция некорректна!");
    private void HandleDuctDrawing(SingleEntity entity) => Debug.Log("[SimBridge Editor] Начало короба зафиксировано. Тяните мышь!");
}