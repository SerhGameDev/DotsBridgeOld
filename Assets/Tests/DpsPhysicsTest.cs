using NUnit.Framework;
using SimBridge.Core.Time;
using SimVent.Components;
using SimVent.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace SimVent.Tests
{
    public class DpsPhysicsTest
    {
        private World _world;
        private EntityManager _em;
        private SystemHandle _physicsSystemHandle;

        [SetUp]
        public void Setup()
        {
            _world = new World("DpsTestWorld");
            _em = _world.EntityManager;
            _physicsSystemHandle = _world.CreateSystem<DuctPhysicsSystem>();

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
                _physicsSystemHandle.Update(_world.Unmanaged);
        }

        // =========================================================================
        // ТЕСТ: ПОЧЕМУ DPS ЛОМАЕТСЯ НА ПОЛНОЙ МОЩНОСТИ
        // =========================================================================
        [Test]
        public void Dps_ShouldTriggerOnPressure_EvenWhenFlowDropsToZero()
        {
            // Улица (Давление 0)
            var street = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(street, new AirNodeComponent { IsInfinite = true, Pressure = 0f });

            // Абсолютно герметичная комната (LeakFactor = 0)
            var room = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(room, new AirNodeComponent { Volume = 100f, LeakFactor = 0f });

            // Труба
            var duct = _em.CreateEntity(typeof(AirDuctComponent), typeof(DuctStateComponent));
            _em.SetComponentData(duct, new AirDuctComponent { SourceNode = street, TargetNode = room });
            _em.SetComponentData(duct, new DuctStateComponent { TotalResistance = 0.5f });

            // Вентилятор выходит на 100% (500 Па)
            var fan = _em.CreateEntity(typeof(FanComponent));
            _em.SetComponentData(fan, new FanComponent { TargetDuct = duct, CurrentSpeed = 1f, MaxPressure = 500f });

            // Ждем, пока вентилятор "надует" герметичную комнату (около 2-3 минут симуляции)
            RunSimulation(10000);

            var ductData = _em.GetComponentData<AirDuctComponent>(duct);
            var roomData = _em.GetComponentData<AirNodeComponent>(room);
            var stateData = _em.GetComponentData<DuctStateComponent>(duct);

            UnityEngine.Debug.Log($"[Анализ DPS] Поток: {ductData.CurrentFlowRate:F1} м³/ч, Напор вентилятора: {stateData.FanPressureBoost:F1} Па, Давление в комнате: {roomData.Pressure:F1} Па");

            // 1. ПРОВЕРКА ОШИБКИ ПОТОКА
            // Поток должен упасть до 0, потому что комната накачалась до 500 Па и воздух больше не лезет.
            // Если ваш DPS привязан к этому параметру, он выдаст FALSE (аварию).
            Assert.AreEqual(0f, ductData.CurrentFlowRate, 1.0f, "Поток не остановился!");

            // 2. ПРОВЕРКА ПРАВИЛЬНОГО DPS (ПО ДАВЛЕНИЮ)
            // Реальный перепад давления (Delta P), который создает вентилятор прямо сейчас.
            // В нашей архитектуре это значение хранится в DuctStateComponent.FanPressureBoost!
            float actualDeltaPressure = stateData.FanPressureBoost;

            // Проверяем, что перепад давления равен 500 Па. 
            // Реальный DPS сработает (выдаст TRUE), если этот перепад больше порога (например, 50 Па).
            Assert.AreEqual(500f, actualDeltaPressure, 1.0f, "Вентилятор не создает давление!");
            Assert.IsTrue(actualDeltaPressure > 50f, "DPS должен быть ВКЛЮЧЕН, так как перепад давления огромный!");
        }
    }
}