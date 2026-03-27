using Unity.Entities;
using UnityEngine;
using SimBridge.Core.Time;

namespace SimOil.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation )] // Добавил Default для теста
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(FluidFlowSystem))]
    public partial struct FlowDebugSystem : ISystem
    {
        private double _nextTickTime;

        public void OnUpdate(ref SystemState state)
        {
            // 1. Проверка: вообще ли заходит в Update?
            // Мы не будем спамить это сообщение, выведем его раз в 2 секунды, даже если симуляция стоит
            bool isHeartbeat = SystemAPI.Time.ElapsedTime > _nextTickTime - 0.1f; 

            // 2. Проверка синглтонов
            if (!SystemAPI.HasSingleton<SimulationOilControl>())
            {
                if(isHeartbeat) Debug.LogWarning("[FlowDebug] ОШИБКА: Нет синглтона SimulationOilControl в мире: " + state.World.Name);
                return;
            }

            if (!SystemAPI.HasSingleton<SimulationTimeComponent>())
            {
                if(isHeartbeat) Debug.LogWarning("[FlowDebug] ОШИБКА: Нет синглтона SimulationTimeComponent");
                return;
            }

            var oilControl = SystemAPI.GetSingleton<SimulationOilControl>();
            
            // 3. Проверка флага IsRunning
            if (!oilControl.IsRunning)
            {
                if(isHeartbeat) Debug.Log("[FlowDebug] Симуляция стоит (IsRunning = false)");
                return;
            }

            // 4. Таймер (0.5 сек)
            if (SystemAPI.Time.ElapsedTime < _nextTickTime)
                return;

            _nextTickTime = SystemAPI.Time.ElapsedTime + 0.5f;

            // 5. Проверка наличия данных через Query
            var mixtureQuery = SystemAPI.QueryBuilder().WithAll<FluidMixture>().Build();
            var linkQuery = SystemAPI.QueryBuilder().WithAll<FluidLink>().Build();

            Debug.Log($"<color=cyan>=== DEBUG TICK (World: {state.World.Name}) ===</color>");
            Debug.Log($"Найдено узлов: {mixtureQuery.CalculateEntityCount()}, связей: {linkQuery.CalculateEntityCount()}");

            if (mixtureQuery.IsEmpty)
            {
                Debug.LogError("[FlowDebug] Сущности с FluidMixture не найдены!");
            }

            // --- ВЫВОД ДАННЫХ ---
            
            foreach (var (mixture, entity) in SystemAPI.Query<RefRO<FluidMixture>>().WithEntityAccess())
            {
                Debug.Log($"<color=white>● Узел {entity.Index}:</color> M={mixture.ValueRO.TotalMass:F1}, P={mixture.ValueRO.Pressure:F2}");
            }

            foreach (var (link, entity) in SystemAPI.Query<RefRO<FluidLink>>().WithEntityAccess())
            {
                Debug.Log($"<color=yellow>→ Связь {entity.Index}:</color> Flow={link.ValueRO.CurrentFlowRateMass:F3} (от {link.ValueRO.NodeA.Index} к {link.ValueRO.NodeB.Index})");
            }
        }
    }
}