using Unity.Entities;

namespace SimElectric.Bridge
{
    // Вешаем этот компонент на любое конечное устройство (вентилятор, ТЭН, заслонку)
    public struct ElectricalDeviceInterface : IComponentData
    {
        public Entity PowerTerminal;    // Клемма, откуда берем питание
        public float ActivationVoltage; // Напряжение, необходимое для старта (например, 24V)
        public bool IsPowered;          // Итоговый статус: есть питание или нет
    }
}