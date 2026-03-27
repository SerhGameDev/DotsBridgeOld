using Unity.Entities;

namespace SimElectric.Interaction
{
    public struct PushButton : IComponentData
    {
        public bool IsMomentary;      // true = с пружиной (возвращается), false = с фиксацией
        public bool IsPressed;        // Нажата ли прямо сейчас
        public float AutoReleaseDelay;// Через сколько секунд пружина откинет кнопку (например, 0.3с)
        public float CurrentTimer;    // Внутренний таймер
    }
}