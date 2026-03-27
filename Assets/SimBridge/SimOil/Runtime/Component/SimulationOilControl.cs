using Unity.Entities;

namespace SimOil
{
    // Глобальные настройки симуляции. Будет использоваться как Singleton.
    public struct SimulationOilControl : IComponentData
    {
        public bool IsRunning;
        public float TimeScale;
        
        public float AmbientTemperature; 
    }
    // УПРАВЛЯЮЩИЙ СИГНАЛ (Inputs)
    // В этот компонент пишет либо простая UI-кнопка (Клиент -> RPC -> Сервер), 
    // либо сложная система симуляции шкафа ПЛК на самом сервере.
    // Для самой задвижки или печи НЕВАЖНО, кто сюда записал данные.
    public struct EquipmentControlSignal : IComponentData
    {
        public bool IsPowered;      // Подано ли питание на устройство
        public float TargetValue;   // Целевое значение (например: открытие задвижки 0.0-1.0, или мощность ТЭНа 0.0-1.0)
    }

    // ФИЗИЧЕСКОЕ СОСТОЯНИЕ (Outputs)
    // Рассчитывается строго на сервере. Например, тяжелая задвижка открывается 10 секунд.
    // Сервер плавно меняет CurrentValue, стремясь к TargetValue из EquipmentControlSignal.
    public struct EquipmentPhysicalState : IComponentData
    {
        public float CurrentValue;  // Текущее физическое значение (открытие или текущая теплоотдача)
        public bool IsFaulty;       // Флаг поломки (авария)
    }
    // Компонент насоса. Размещается на той же сущности, что и FluidLink.
}