using Unity.Entities;
using Unity.NetCode;

namespace SimOil
{
    [GhostComponent]
    public struct FluidMixture : IComponentData
    {
        [GhostField] public float TotalMass;       // кг
        [GhostField] public float Temperature;     // °C
        [GhostField] public float Pressure;        // МПа

        // Фракции (Сумма всегда должна быть = 1.0)
        public float FractionGas;            // С1-С4
        public float FractionLightNaphtha;   // Н.К. – 85°C
        public float FractionHeavyNaphtha;   // 85 – 180°C
        public float FractionKerosene;       // 180 – 230°C
        public float FractionLightDiesel;    // 230 – 280°C
        public float FractionHeavyDiesel;    // 280 – 350°C
        public float FractionMazut;          // > 350°C
        public float FractionWater;          // Вода / Пар
    }
    [GhostComponent]
    // Компонент связи двух логических узлов
    public struct FluidLink : IComponentData
    {
        public Entity NodeA;
        public Entity NodeB;
        
        // Физические параметры трубы
        [GhostField] public float CrossSectionArea; // Площадь сечения (м²)
        [GhostField] public float FrictionFactor;   // Коэффициент трения (замедляет поток)
        
        // Динамический параметр (рассчитывается системой)
        // Если > 0, течет от A к B. Если < 0, течет от B к A.
        public float CurrentFlowRateMass; // Текущий массовый расход (кг/с)
    }

    [GhostComponent]
    public struct ValveLink : IComponentData
    {
        // 0.0 - полностью закрыта, 1.0 - полностью открыта
        [GhostField] public float Openness; 
    }
}