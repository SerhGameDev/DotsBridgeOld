using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Placement.Test
{
    public class PlacementTestController : MonoBehaviour
    {
        [SerializeField] private float _step = 0.1f;
        private BridgeWorld _bridge;
        private SingleEntity _currentGhost;

        void Update()
        {
            if (_bridge == null)
            {
                _bridge = EntityBridge.InCurrentWorld();
                if (_bridge == null) return;
            }

            var query = _bridge.Manager.CreateEntityQuery(typeof(PlacementSettings));
            if (query.IsEmptyIgnoreFilter) return;
            
            var settings = query.GetSingleton<PlacementSettings>();

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (_currentGhost.Entity != Entity.Null) 
                    _currentGhost.Destroy();

                _currentGhost = SingleEntity.Instantiate(_bridge, settings.GhostPrefab);
                
                // Добавляем потерянный тег с начальным состоянием IsValid = true
                _currentGhost.AddComponent(new GhostTag { IsValid = true });
                _currentGhost.AddComponent<PlaceableTag>();
                _currentGhost.AddComponent(new GridSnapSettings { IsEnabled = true, Step = _step });
                
                // Если на префабе не было компонента PlacementPivot, можно добавить его программно как фолбек
                if (!_currentGhost.HasComponent<PlacementPivot>())
                {
                    _currentGhost.AddComponent(new PlacementPivot { Value = new float3(0, 0.5f, 0) });
                }
                
                Debug.Log("[SimBridge Editor] Выбран компонент. ЛКМ - разместить, G - вкл/выкл сетку.");
            }

            // --- ПЕРЕКЛЮЧЕНИЕ СЕТКИ ---
            if (Input.GetKeyDown(KeyCode.G) && _currentGhost.Entity != Entity.Null)
            {
                var snap = _currentGhost.GetComponent<GridSnapSettings>();
                snap.IsEnabled = !snap.IsEnabled;
                snap.Step = _step;
                _currentGhost.SetComponent(snap);
                Debug.Log($"[SimBridge Editor] Привязка к сетке: {(snap.IsEnabled ? "ВКЛ" : "ВЫКЛ")}");
            }

            if (Input.GetMouseButtonDown(0) && _currentGhost.Entity != Entity.Null)
            {
                var ghostTag = _currentGhost.GetComponent<GhostTag>();
                
                if (ghostTag.IsValid)
                {
                    _bridge.Manager.RemoveComponent<GhostTag>(_currentGhost.Entity);
                    Debug.Log("[SimBridge Editor] Объект зафиксирован на поверхности!");
                    _currentGhost = default; 
                }
                else
                {
                    Debug.LogWarning("[SimBridge Editor] Ошибка: Место занято! Пересечение с другим объектом.");
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) && _currentGhost.Entity != Entity.Null)
            {
                _currentGhost.Destroy();
                _currentGhost = default;
            }
        }
    }
}