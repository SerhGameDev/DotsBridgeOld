using NUnit.Framework;
using SimBridge.Core.Time;
using SimVent.Components;
using SimVent.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace SimVent.Tests
{
    public class SensorPhysicsTest
    {
        private World _world;
        private EntityManager _em;
        private SystemHandle _sensorSystemHandle;

        [SetUp]
        public void Setup()
        {
            _world = new World("SensorTestWorld");
            _em = _world.EntityManager;
            // В этом тесте нам нужна только система датчиков
            _sensorSystemHandle = _world.CreateSystem<SensorSystem>();

            var timeEntity = _em.CreateEntity(typeof(SimulationTimeComponent));
            _em.SetComponentData(timeEntity, new SimulationTimeComponent { FixedStep = 0.02f });
        }

        [TearDown]
        public void TearDown()
        {
            _world.Dispose();
        }

        private void RunSimulation(int ticks)
        {
            for (int i = 0; i < ticks; i++)
            {
                _sensorSystemHandle.Update(_world.Unmanaged);
            }
        }

        // =========================================================================
        // ТЕСТ 1: МГНОВЕННЫЙ ДАТЧИК (Без инерции)
        // =========================================================================
        [Test]
        public void NodeSensor_WithoutInertia_ShouldReadInstantly()
        {
            var room = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(room, new AirNodeComponent { Temperature = 25.5f });

            var sensor = _em.CreateEntity(typeof(NodeTemperatureSensorComponent));
            _em.SetComponentData(sensor, new NodeTemperatureSensorComponent
            {
                TargetNode = room,
                MeasuredTemperature = 0f,
                SensorTimeConstant = 0f // Мгновенный
            });

            RunSimulation(1); // Всего 1 кадр симуляции!

            var sensorData = _em.GetComponentData<NodeTemperatureSensorComponent>(sensor);
            Assert.AreEqual(25.5f, sensorData.MeasuredTemperature, "Датчик без инерции должен показывать температуру моментально!");
        }

        // =========================================================================
        // ТЕСТ 2: ДАТЧИК С ТЕПЛОВОЙ ИНЕРЦИЕЙ (Реалистичный)
        // =========================================================================
        [Test]
        public void NodeSensor_WithInertia_ShouldMatchThermalDelay()
        {
            // Комната мгновенно нагрелась до 100 градусов
            var room = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(room, new AirNodeComponent { Temperature = 100f });

            var sensor = _em.CreateEntity(typeof(NodeTemperatureSensorComponent));
            _em.SetComponentData(sensor, new NodeTemperatureSensorComponent
            {
                TargetNode = room,
                MeasuredTemperature = 0f, // Датчик стартует с нуля
                SensorTimeConstant = 5f   // Инерция 5 секунд
            });

            // Запускаем симуляцию ровно на 5 секунд (250 тиков по 0.02с)
            RunSimulation(250);

            var sensorData = _em.GetComponentData<NodeTemperatureSensorComponent>(sensor);

            // МАТЕМАТИКА: За время, равное 1 ТАУ (5 секунд), 
            // датчик должен пройти ~63.2% пути от 0 до 100.
            UnityEngine.Debug.Log($"Температура на датчике через 5 секунд: {sensorData.MeasuredTemperature:F1} °C");

            Assert.AreEqual(63.2f, sensorData.MeasuredTemperature, 1.0f, "Тепловая инерция датчика считается неверно!");

            // Дадим ему 40 секунд (2000 тиков по 0.02с), чтобы он точно дошел до финала (8 ТАУ)
            RunSimulation(2000);
            sensorData = _em.GetComponentData<NodeTemperatureSensorComponent>(sensor);

            // Теперь он точно будет в пределах 100.0 +/- 0.5
            Assert.AreEqual(100f, sensorData.MeasuredTemperature, 0.5f, "Датчик не вышел на финальную температуру!");
        }
    }
}