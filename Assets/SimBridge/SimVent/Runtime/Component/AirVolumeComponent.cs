using Unity.Entities;

namespace SimVent.Components
{
    // Базовый узел (кусок воздуховода, камера смешения или комната)
    public struct AirVolumeComponent : IComponentData
    {
        public float Temperature; // Текущая температура (°C)
        public float Mass;        // Масса воздуха (кг) - влияет на скорость нагрева (инерция)
        public float Pressure;    // Давление (Па) - нужно для расчета расхода
        public float Volume;      // Объем (м³)
    }

    // Компонент водяного калорифера (нагревателя)
    public struct WaterHeaterComponent : IComponentData
    {
        public float ValvePosition;     // Открытие клапана воды (0.0 - 1.0) от ПЛК/FBD
        public float SupplyWaterTemp;   // Температура подающей воды от котельной (°C)
        public float MetalTemperature;  // Текущая температура самого радиатора (°C)
        public float HeatCapacity;      // Теплоемкость радиатора (определяет инерцию нагрева)
        public float HeatTransferCoef;  // Коэффициент теплопередачи воздуху
    }
}