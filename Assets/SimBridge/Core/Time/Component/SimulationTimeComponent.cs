using Unity.Entities;

namespace SimBridge.Core.Time
{
    // Singleton-компонент для управления временем всей симуляции
    public struct SimulationTimeComponent : IComponentData
    {
        public double TotalTime;          // Общее прошедшее время симуляции (в секундах)
        public float TimeScale;           // Множитель (0 - пауза, 1 - реалтайм, 10 - ускорение х10)

        // Переменные для обеспечения детерминированной физики при перемотке
        public float FixedStep;           // Размер фиксированного шага (например, 0.02с = 50 Гц)
        public float AccumulatedTime;     // Накопленное время для обработки фиксированных шагов
    }
}