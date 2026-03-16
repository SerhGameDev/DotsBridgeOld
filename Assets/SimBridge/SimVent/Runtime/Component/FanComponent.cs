using Unity.Entities;

namespace SimVent.Components
{
    // 1. Вентилятор (Привод)
    public struct FanComponent : IComponentData
    {
        public Entity TargetDuct;     // <-- ДОБАВЛЕНО: Ссылка на трубу
        public bool RunCommand;
        public float CurrentSpeed;
        public float SpinUpTime;
        public float SpinDownTime;
        public float MaxFlowCapacity;
        public float MaxPressure;
    }
    // 3. Электрический нагреватель (ТЭН)
    public struct ElectricHeaterComponent : IComponentData
    {
        public Entity TargetDuct;
        public float TargetPower;
        public float MaxPowerKW;           // Максимальная мощность (15.0)
        public float ControlSignal;        // СИГНАЛ ОТ ПЛК: от 0.0 (выкл) до 1.0 (100%)
        public float CurrentHeatOutput;    // Текущие кВт (то, что идет в физику)
        public float HeatingTimeConstant;  // Инерция
        public float OverheatThreshold;    // Порог аварии (70)
        public bool IsOverheated;          // Статус аварии
    }


    // Узел (Улица, Комната, Тройник)
    public struct AirNodeComponent : IComponentData
    {
        public bool IsInfinite;
        public float Temperature;
        public float Volume;
        public float HeatLossFactor;
        public float TargetStreetTemp;
        public float Pressure;
        public float LeakFactor;
    }

    // Труба (переносит воздух)
    public struct AirDuctComponent : IComponentData
    {
        public Entity SourceNode;
        public Entity TargetNode;
        public float CurrentFlowRate;
        public float AirTemperature;
    }
    // Состояние трубы в текущем кадре (буфер для сбора данных со всего оборудования)
    public struct DuctStateComponent : IComponentData
    {
        public float FanPressureBoost; // Паскали от вентиляторов
        public float TotalResistance;  // Сопротивление от заслонок
        public float TotalHeatKW;      // Нагрев от ТЭНов
    }

    // Датчик температуры
    public struct TemperatureSensorComponent : IComponentData
    {
        public Entity TargetDuct;
        public float MeasuredTemperature;
        public float SensorTimeConstant;
    }
    // 2. Воздушная заслонка (Привод)
    public struct DamperComponent : IComponentData
    {
        public Entity TargetDuct;
        public float TargetOpening;
        public float CurrentOpening;
        public float TransitTime;
    }
}