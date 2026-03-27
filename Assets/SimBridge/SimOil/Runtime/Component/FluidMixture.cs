using Unity.Entities;

namespace SimOil
{
    public struct FluidMixture : IComponentData
    {
        public float TotalMass;       // кг
        public float Temperature;     // °C
        public float Pressure;        // МПа

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
    // Компонент связи двух логических узлов
    public struct FluidLink : IComponentData
    {
        public Entity NodeA;
        public Entity NodeB;
        
        // Физические параметры трубы
        public float CrossSectionArea; // Площадь сечения (м²)
        public float FrictionFactor;   // Коэффициент трения (замедляет поток)
        
        // Динамический параметр (рассчитывается системой)
        // Если > 0, течет от A к B. Если < 0, течет от B к A.
        public float CurrentFlowRateMass; // Текущий массовый расход (кг/с)
    }

    public struct ValveLink : IComponentData
    {
        // 0.0 - полностью закрыта, 1.0 - полностью открыта
        public float Openness; 
    }
}