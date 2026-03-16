using Unity.Entities;

namespace SimVent.Components
{
    public struct SimVentConfigComponent : IComponentData
    {
        // --- Аэродинамика ---
        public float BaseDuctResistance; // Базовое сопротивление 1 сегмента трубы (было 0.5)
        public float DuctFrictionPerMeter; // Сопротивление на 1 метр трубы (Volume узла)

        // --- Термодинамика ---
        public float AirHeatCapacityConstant; // Константа для расчета кВт (было 2985)
        public float DeadHeadOverheatRate;    // Скорость перегрева стоячего воздуха (было 50)
        public float DeadHeadCooldownRate;    // Скорость остывания стоячей трубы (было 0.05)
        public float PressureMultiplier;      // Множитель роста давления в узле (было 10000)
    }
}