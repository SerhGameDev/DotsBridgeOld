using Unity.Entities;

namespace SimElectric
{
    // Компонент провода (физического соединения)
    public struct Wire : IComponentData
    {
        public Entity NodeA;         // Ссылка на первую клемму
        public Entity NodeB;         // Ссылка на вторую клемму

        public float MaxCurrent;     // Максимальный ток до перегорания (для симуляции КЗ)
        public float Resistance;       // Сопротивление самого провода (Ом)
        public bool IsBroken;        // Физический обрыв (КЗ или удаление)
        public bool IsConducting;    // Логическая проводимость (для контактов реле)
        public float CurrentFlow;      // Текущий ток, текущий по проводу (Амперы)
    }
}
