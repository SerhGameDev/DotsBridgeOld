using Unity.Entities;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SimBridge.Core.Time
{
    // MonoBehaviour, который мы вешаем на GameObject в сцене
    public class SimulationTimeAuthoring : MonoBehaviour
    {
        [Title("Управление временем симуляции", "Настройки ядра SimBridge")]
        [InfoBox("TimeScale = 1 (реальное время). TimeScale = 100 (ускорение в 100 раз). 0 = пауза.")]
        
        [Range(0f, 1000f)]
        [GUIColor(0.8f, 1f, 0.8f)]
        public float TimeScale = 1f;

        [Title("Физика и точность")]
        [PropertyTooltip("Фиксированный шаг физики в секундах. 0.02 = 50 тиков в секунду.")]
        [MinValue(0.001f)]
        public float FixedStep = 0.02f;

        // Baker - это внутренний класс DOTS, который переводит данные из ООП в DOD (ECS)
        class SimulationTimeBaker : Baker<SimulationTimeAuthoring>
        {
            public override void Bake(SimulationTimeAuthoring authoring)
            {
                // TransformUsageFlags.None означает, что этой сущности не нужен Transform
                // (она не имеет позиции в пространстве, это просто глобальный менеджер)
                var entity = GetEntity(TransformUsageFlags.None);
                
                AddComponent(entity, new SimulationTimeComponent
                {
                    TimeScale = authoring.TimeScale,
                    FixedStep = authoring.FixedStep,
                    TotalTime = 0,
                    AccumulatedTime = 0
                });
            }
        }
    }
}