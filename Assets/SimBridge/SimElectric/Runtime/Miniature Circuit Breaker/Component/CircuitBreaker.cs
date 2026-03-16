using Unity.Entities;

namespace SimElectric
{
    public struct CircuitBreaker : IComponentData
    {
        public float RatedCurrent; // Номинальный ток (Амперы), при котором выбивает автомат
        public bool IsTripped;     // Состояние: выбило (true) или взведен (false)
    }
}