using Unity.Entities;

namespace SimVent.Components
{
    // Компонент датчика перепада давления (DPS - Differential Pressure Switch)
    public struct DpsSensorComponent : IComponentData
    {
        public Entity TargetDuct;     // Воздуховод, в котором меряем поток

        public float Setpoint;        // Уставка срабатывания (например, 1000 м³/ч)
        public float FlickerBand;     // Зона турбулентности (амплитуда "дребезга")

        // Выходной сигнал для ПЛК
        public bool OutputSignal;     // Дискретный вход (DI) для контроллера
    }
    // Датчик температуры для помещения/улицы
    public struct NodeTemperatureSensorComponent : IComponentData
    {
        public Entity TargetNode;         // Ссылка на комнату или улицу
        public float MeasuredTemperature; // То, что видит ПЛК
        public float SensorTimeConstant;  // Инерция в секундах (ТАУ). 0 = мгновенно.
    }
}