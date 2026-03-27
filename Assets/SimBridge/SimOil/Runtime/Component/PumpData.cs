using Unity.Entities;

namespace SimOil
{
    public struct PumpData : IComponentData
    {
        // Максимальная добавка к давлению (напор насоса), которую он может создать (в МПа)
        public float MaxPressureBoost; 
        
        // Значение от 0.0 до 1.0. 
        // В будущем сюда будет писать система контроллеров ПЛК (плавный пуск).
        public float CurrentPower; 
    }
}