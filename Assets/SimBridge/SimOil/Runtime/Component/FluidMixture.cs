using Unity.Entities;
using Unity.NetCode;

namespace SimOil
{
    public struct FluidMixture : IComponentData
    {
        public float TotalMass;       // кг
        public float Temperature;     // °C
        public float Pressure;        // МПа

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
    public partial struct NetSync_FluidNode : IComponentData
    {
        [GhostField] public float TotalMass;
        [GhostField] public float Pressure;
        [GhostField] public float Temperature;
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

    [GhostComponent]
    public struct ValveLink : IComponentData
    {
        // 0.0 - полностью закрыта, 1.0 - полностью открыта
        [GhostField] public float Openness; 
    }
    
    public enum FractionType : byte
    {
        None = 0,
        Gas = 1,
        LightNaphtha = 2,
        HeavyNaphtha = 3,
        Kerosene = 4,
        LightDiesel = 5,
        HeavyDiesel = 6,
        Mazut = 7,
        Water = 8
    }

    // Фильтр для трубы. Размещается на той же сущности, что и FluidLink.
    public struct FractionFilter : IComponentData
    {
        public FractionType AllowedFraction;
        
        // Минимальная температура в узле-источнике, при которой фракция 
        // начинает переходить в пар и проникать в эту трубу
        public float MinBoilingTemperature; 
    }
}