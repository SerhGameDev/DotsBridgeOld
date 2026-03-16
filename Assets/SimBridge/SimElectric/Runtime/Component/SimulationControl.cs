using Unity.Entities;

namespace SimElectric
{
    public struct SimulationControl : IComponentData
    {
        public bool IsRunning;       // Работает ли симуляция непрерывно
        public bool StepNextFrame;   // Флаг для выполнения ровно одного такта (для покадрового дебага)

    }
}
