using Unity.Entities;
using UnityEngine;

namespace SimOil
{
    public class SimulationOilControlAuthoring : MonoBehaviour
    {
        [Tooltip("Запущена ли симуляция потоков и реакций")]
        public bool isRunning = true;
        
        [Tooltip("Множитель времени для симуляции процессов")]
        [Range(0f, 10f)] 
        public float timeScale = 1.0f;

        [Tooltip("Температура окружающей среды (градусы Цельсия)")]
        public float ambientTemperature = 20.0f;

        public class Baker : Baker<SimulationOilControlAuthoring>
        {
            public override void Bake(SimulationOilControlAuthoring authoring)
            {
                // Для синглтона с настройками трансформ не нужен
                var entity = GetEntity(TransformUsageFlags.None);
                
                AddComponent(entity, new SimulationOilControl
                {
                    IsRunning = authoring.isRunning,
                    TimeScale = authoring.timeScale,
                    AmbientTemperature = authoring.ambientTemperature
                });
            }
        }
    }
}