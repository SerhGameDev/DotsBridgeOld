using NUnit.Framework;
using SimBridge.Core.Time;
using SimVent.Components;
using SimVent.Systems;
using Unity.Entities;

namespace SimVent.Tests
{
    public class SystemIntegrationTest
    {
        private World _world;
        private EntityManager _em;
        private SystemHandle _physicsSystem;
        private SystemHandle _actuatorSystem; // Добавляем систему приводов

        [SetUp]
        public void Setup()
        {
            _world = new World("IntegrationTestWorld");
            _em = _world.EntityManager;

            // Создаем ОБЕ системы
            _actuatorSystem = _world.CreateSystem<ActuatorSystem>();
            _physicsSystem = _world.CreateSystem<DuctPhysicsSystem>();

            var timeEntity = _em.CreateEntity(typeof(SimulationTimeComponent));
            _em.SetComponentData(timeEntity, new SimulationTimeComponent { FixedStep = 0.02f });
        }

        [TearDown]
        public void TearDown()
        {
            _world.Dispose();
        }

        [Test]
        public void ActuatorSystem_WithZeroTarget_ShouldCloseDamper_AndStopFlow()
        {
            // Собираем простую трубу с вентилятором и заслонкой
            var street = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(street, new AirNodeComponent { IsInfinite = true, Pressure = 0f });

            var room = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(room, new AirNodeComponent { Volume = 100f, LeakFactor = 0.5f });

            var duct = _em.CreateEntity(typeof(AirDuctComponent), typeof(DuctStateComponent));
            _em.SetComponentData(duct, new AirDuctComponent { SourceNode = street, TargetNode = room });
            _em.SetComponentData(duct, new DuctStateComponent { TotalResistance = 0.5f });

            // Вентилятор ВКЛЮЧЕН
            var fan = _em.CreateEntity(typeof(FanComponent));
            _em.SetComponentData(fan, new FanComponent { TargetDuct = duct, RunCommand = true, CurrentSpeed = 1f, MaxPressure = 500f });

            // ЗАСЛОНКА: Текущее открытие = 1 (открыта), но ЦЕЛЕВОЕ = 0 (закрыта)
            var damper = _em.CreateEntity(typeof(DamperComponent));
            _em.SetComponentData(damper, new DamperComponent
            {
                TargetDuct = duct,
                CurrentOpening = 1f,
                TargetOpening = 0f, // <-- Вот она, возможная причина поломки на сцене!
                TransitTime = 1f
            });

            // Симулируем 2 секунды (100 тиков)
            for (int i = 0; i < 100; i++)
            {
                // Сначала отрабатывают приводы, потом физика
                _actuatorSystem.Update(_world.Unmanaged);
                _physicsSystem.Update(_world.Unmanaged);
            }

            var damperData = _em.GetComponentData<DamperComponent>(damper);
            var ductData = _em.GetComponentData<AirDuctComponent>(duct);

            UnityEngine.Debug.Log($"После 2 секунд: Открытие заслонки = {damperData.CurrentOpening}, Поток = {ductData.CurrentFlowRate}");

            // Тест проверяет, что ActuatorSystem успешно закрыл заслонку, и физика остановила поток
            Assert.IsTrue(damperData.CurrentOpening < 0.05f, "ActuatorSystem не закрыл заслонку!");
            Assert.IsTrue(ductData.CurrentFlowRate < 10f, "Поток не остановился, хотя заслонка закрыта!");
        }
    }
}