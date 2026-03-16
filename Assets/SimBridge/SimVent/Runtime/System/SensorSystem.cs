using Unity.Entities;
using Unity.Mathematics;
using SimVent.Components;
using SimBridge.Core.Time;

namespace SimVent.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(DuctPhysicsSystem))] // Датчики читают данные ПОСЛЕ того, как отработает физика
    public partial struct SensorSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.GetSingleton<SimulationTimeComponent>().FixedStep; // Никаких умножений!

            var nodeLookup = SystemAPI.GetComponentLookup<AirNodeComponent>(true);
            var ductLookup = SystemAPI.GetComponentLookup<AirDuctComponent>(true);

            // Обновляем датчики помещений
            new UpdateNodeSensorsJob
            {
                NodeLookup = nodeLookup,
                DeltaTime = dt
            }.ScheduleParallel();

            // Обновляем датчики в трубах
            new UpdateDuctSensorsJob
            {
                DuctLookup = ductLookup,
                DeltaTime = dt
            }.ScheduleParallel();
        }
    }

    public partial struct UpdateNodeSensorsJob : IJobEntity
    {
        [Unity.Collections.ReadOnly] public ComponentLookup<AirNodeComponent> NodeLookup;
        public float DeltaTime;

        void Execute(ref NodeTemperatureSensorComponent sensor)
        {
            if (!NodeLookup.HasComponent(sensor.TargetNode)) return;

            float actualTemp = NodeLookup[sensor.TargetNode].Temperature;

            if (sensor.SensorTimeConstant <= 0f)
            {
                sensor.MeasuredTemperature = actualTemp; // Мгновенное чтение
            }
            else
            {
                // Имитация тепловой инерции корпуса датчика
                float speed = DeltaTime / sensor.SensorTimeConstant;
                sensor.MeasuredTemperature = math.lerp(sensor.MeasuredTemperature, actualTemp, speed);
            }
        }
    }

    public partial struct UpdateDuctSensorsJob : IJobEntity
    {
        [Unity.Collections.ReadOnly] public ComponentLookup<AirDuctComponent> DuctLookup;
        public float DeltaTime;

        void Execute(ref TemperatureSensorComponent sensor)
        {
            if (!DuctLookup.HasComponent(sensor.TargetDuct)) return;

            float actualTemp = DuctLookup[sensor.TargetDuct].AirTemperature;

            if (sensor.SensorTimeConstant <= 0f)
            {
                sensor.MeasuredTemperature = actualTemp;
            }
            else
            {
                float speed = DeltaTime / sensor.SensorTimeConstant;
                sensor.MeasuredTemperature = math.lerp(sensor.MeasuredTemperature, actualTemp, speed);
            }
        }
    }
}