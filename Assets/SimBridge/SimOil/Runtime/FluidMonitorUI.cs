using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using DotsBridge; // Оставляем только для доступа к миру

namespace SimOil.UI
{
    public class FluidDebugUI : MonoBehaviour
    {
        public bool ShowUI = true;
        public int FontSize = 20;

        private EntityQuery _networkQuery;

        private void OnGUI()
        {
            if (!ShowUI) return;

            // 1. Проверяем мост
            if (!ClientBridge.IsActive || ClientBridge.World() == null)
            {
                GUI.Label(new Rect(10, 10, 400, 30), "КЛИЕНТ: Ожидание мира...");
                return;
            }

            var em = ClientBridge.World().Manager;

            // 1. Создаем БЕЗЖАЛОСТНЫЙ запрос, который ищет даже скрытые и отключенные сущности
            if (_networkQuery == default)
            {
                _networkQuery = em.CreateEntityQuery(new EntityQueryDesc 
                {
                    All = new ComponentType[] { typeof(NetSync_FluidNode) },
                    Options = EntityQueryOptions.Default // <--- ВОТ ГЛАВНЫЙ СЕКРЕТ
                });
            }

            GUI.skin.label.fontSize = FontSize;
            GUI.color = Color.cyan;

            // 2. Считаем ВСЕ сущности в мире, чтобы понять, туда ли мы смотрим
            int totalEntitiesInWorld = em.UniversalQuery.CalculateEntityCount();
            int count = _networkQuery.CalculateEntityCount();

            if (count == 0)
            {
                GUI.Label(new Rect(10, 10, 800, 40), $"КЛИЕНТ: Мир {ClientBridge.World().World.Name} | Всего сущностей в мире: {totalEntitiesInWorld} | Узлов: 0");
                return;
            }

            // 3. Достаем данные
            GUILayout.BeginArea(new Rect(10, 10, 600, 800));
            GUILayout.Label($"=== СЕТЕВОЙ МОНИТОР (Узлов: {count}) ===");
            GUILayout.Space(10);

            // Получаем массивы сущностей и данных напрямую из памяти
            using var entities = _networkQuery.ToEntityArray(Allocator.Temp);
            using var netDataArray = _networkQuery.ToComponentDataArray<NetSync_FluidNode>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var netData = netDataArray[i];

                GUILayout.BeginVertical("box");
                GUILayout.Label($"УЗЕЛ [Entity: {entity.Index}]");
                
                // Раскрасим значения для наглядности
                GUI.color = netData.TotalMass > 0 ? Color.green : Color.yellow;
                GUILayout.Label($"  • Масса: {netData.TotalMass:F2} кг");
                
                GUI.color = netData.Pressure > 1.0f ? new Color(1f, 0.5f, 0f) : Color.white;
                GUILayout.Label($"  • Давление: {netData.Pressure:F2} МПа");
                
                GUI.color = Color.cyan;
                GUILayout.Label($"  • Температура: {netData.Temperature:F1} °C");
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }

            GUILayout.EndArea();
        }
    }
}