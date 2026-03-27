using DotsBridge;
using DotsBridge.Interaction;
using DotsBridge.Placement;
using SimElectric;
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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EntityBridge.InCurrentWorld().BeginSpawn(_prefabNameToSpawn).Spawn().BeginPlacement(_step);
            EntityBridge.InCurrentWorld().ToggleCurrentGridSnap();
            Debug.Log("[SimBridge Editor] Выбран компонент. ЛКМ - разместить, G - вкл/выкл сетку.");
        }
        if (Input.GetMouseButtonDown(1)) // 1 - правая кнопка мыши
        {
            EntityBridge.InCurrentWorld().GetEntityUnderMouse<HingeTrigger>().TriggerHinge();
            EntityBridge.InCurrentWorld().GetEntityUnderMouse<PushButtonTrigger>().TriggerButton();
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            bool isSnapActive = EntityBridge.InCurrentWorld().ToggleCurrentGridSnap();
            Debug.Log($"[SimBridge Editor] Привязка к сетке: {(isSnapActive ? "ВКЛ" : "ВЫКЛ")}");
        }

        // --- 3. UI-ИНДИКАЦИЯ ---
        if (EntityBridge.InCurrentWorld().HasActiveGhost())
        {
            bool isValid = EntityBridge.IsCurrentPlacementValid();
            if (isValid != _wasValid)
            {
                _wasValid = isValid;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EntityBridge.InCurrentWorld().HasActiveGhost())
            {
                EntityBridge.CompleteCurrentPlacement();
            }
            else 
            {
                EntityBridge.InCurrentWorld().GetEntityUnderMouse<PlaceableTag>().BeginPlacement(_step);
            }
        }
    }

    // --- ОБРАБОТЧИКИ СОБЫТИЙ ---
    private void HandlePlacementSuccess(SingleEntity entity) => Debug.Log("[SimBridge Editor] Успех: Объект зафиксирован!");
    private void HandlePlacementFailed(SingleEntity entity) => Debug.LogWarning("[SimBridge Editor] Ошибка: Место занято или позиция некорректна!");
    private void HandleDuctDrawing(SingleEntity entity) => Debug.Log("[SimBridge Editor] Начало короба зафиксировано. Тяните мышь!");
}