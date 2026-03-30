using Unity.Entities;

namespace SimOil.Equipment
{
    // Компонент нагревателя (Трубчатая печь, ТЭН или теплообменник)
    public struct HeaterData : IComponentData
    {
        // Максимальная тепловая мощность в киловаттах (кДж/с)
        public float MaxHeatPower; 
        
        // Текущий сигнал управления (от 0.0 до 1.0)
        public float CurrentPower; 
        
        // Условная теплоизоляция узла (0.0 - нет потерь, 1.0 - быстро остывает на ветру)
        public float HeatLossFactor; 
    }
}