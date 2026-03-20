using DotsBridge.Interaction;
using UnityEngine;
using DotsBridge.Placement;

namespace DotsBridge.Test
{
    public class PlacementTestController : MonoBehaviour
    {
        [SerializeField] private float _step = 0.1f;
        [SerializeField] private string _prefabNameToSpawn = "RelayCube"; // Имя префаба из вашего реестра

        private bool _wasValid = true;

        void OnEnable()
        {
            EntityBridge.OnPlacementSuccess += HandlePlacementSuccess;
            EntityBridge.OnPlacementFailed += HandlePlacementFailed;
            EntityBridge.OnDuctDrawingStarted += HandleDuctDrawing;
        }

        void OnDisable()
        {
            EntityBridge.OnPlacementSuccess -= HandlePlacementSuccess;
            EntityBridge.OnPlacementFailed -= HandlePlacementFailed;
            EntityBridge.OnDuctDrawingStarted -= HandleDuctDrawing;
        }
        
        void Update()
        {
            // --- 1. СПАВН НОВОГО ОБЪЕКТА ---
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                EntityBridge.InCurrentWorld().BeginSpawn(_prefabNameToSpawn).Spawn().BeginPlacement(_step);
                Debug.Log("[SimBridge Editor] Выбран компонент. ЛКМ - разместить, G - вкл/выкл сетку.");
            }
            if (Input.GetMouseButtonDown(1)) // 1 - правая кнопка мыши
            {
                EntityBridge.InCurrentWorld().FindWithComponent<HingeState>().ToggleHinge();
            }
            // --- 2. УПРАВЛЕНИЕ СЕТКОЙ ---
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
                    // TODO: Смена материала на красный/зеленый
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

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EntityBridge.CancelCurrentPlacement();
            }
        }

        // --- ОБРАБОТЧИКИ СОБЫТИЙ ---
        private void HandlePlacementSuccess(SingleEntity entity) => Debug.Log("[SimBridge Editor] Успех: Объект зафиксирован!");
        private void HandlePlacementFailed(SingleEntity entity) => Debug.LogWarning("[SimBridge Editor] Ошибка: Место занято или позиция некорректна!");
        private void HandleDuctDrawing(SingleEntity entity) => Debug.Log("[SimBridge Editor] Начало короба зафиксировано. Тяните мышь!");
    }
}