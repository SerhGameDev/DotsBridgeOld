using NUnit.Framework;
using SimBridge.Core.Time;
using SimVent.Components;
using SimVent.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace SimVent.Tests
{
    public class HeaterPhysicsTest
    {
        private World _world;
        private EntityManager _em;
        private SystemHandle _physicsSystemHandle;

        [SetUp]
        public void Setup()
        {
            _world = new World("HeaterTestWorld");
            _em = _world.EntityManager;
            _physicsSystemHandle = _world.CreateSystem<DuctPhysicsSystem>();

            var timeEntity = _em.CreateEntity(typeof(SimulationTimeComponent));
            _em.SetComponentData(timeEntity, new SimulationTimeComponent { FixedStep = 0.02f }); // dt = 0.02
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
                _physicsSystemHandle.Update(_world.Unmanaged);
            }
        }

        // =========================================================================
        // ТЕСТ 1: ИДЕАЛЬНЫЙ НАГРЕВ В ПОТОКЕ ВОЗДУХА
        // =========================================================================
        [Test]
        public void Heater_WithNormalFlow_ShouldReachCalculatedTemperature()
        {
            // Улица 0 °C
            var street = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(street, new AirNodeComponent { IsInfinite = true, Temperature = 0f, Pressure = 0f });

            // Бесконечная комната, чтобы давление не росло и поток был стабильным
            var room = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(room, new AirNodeComponent { IsInfinite = true, Pressure = 0f });

            var duct = _em.CreateEntity(typeof(AirDuctComponent), typeof(DuctStateComponent));
            _em.SetComponentData(duct, new AirDuctComponent { SourceNode = street, TargetNode = room, AirTemperature = 0f });
            _em.SetComponentData(duct, new DuctStateComponent { TotalResistance = 1f }); // Сопротивление 1

            // Даем напор 500 Па. При несгораемом сопротивлении трубы 0.5 поток будет ровно 1000 м³/ч
            var fan = _em.CreateEntity(typeof(FanComponent));
            _em.SetComponentData(fan, new FanComponent { TargetDuct = duct, CurrentSpeed = 1f, MaxPressure = 500f });

            // ТЭН выдает ровно 10 кВт
            var heater = _em.CreateEntity(typeof(ElectricHeaterComponent));
            _em.SetComponentData(heater, new ElectricHeaterComponent { TargetDuct = duct, CurrentHeatOutput = 10f });

            // Ждем 500 тиков (10 секунд), чтобы температура в трубе "догнала" целевую через lerp
            RunSimulation(500);

            var ductData = _em.GetComponentData<AirDuctComponent>(duct);

            // ФОРМУЛА: dT = (10 кВт * 2985) / 1000 м3/ч = 29.85 °C
            Assert.AreEqual(1000f, ductData.CurrentFlowRate, 0.1f, "Поток не равен 1000 м³/ч");
            Assert.AreEqual(29.85f, ductData.AirTemperature, 0.2f, "Температура не соответствует закону нагрева!");
        }

        // =========================================================================
        // ТЕСТ 2: АВАРИЙНЫЙ ПЕРЕГРЕВ СТОЯЧЕГО ВОЗДУХА (ВЕНТИЛЯТОР ВЫКЛЮЧЕН)
        // =========================================================================
        [Test]
        public void Heater_WithNoFlow_ShouldOverheatRapidly()
        {
            var street = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(street, new AirNodeComponent { IsInfinite = true, Temperature = 0f });

            var room = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(room, new AirNodeComponent { IsInfinite = true });

            var duct = _em.CreateEntity(typeof(AirDuctComponent), typeof(DuctStateComponent));
            _em.SetComponentData(duct, new AirDuctComponent { SourceNode = street, TargetNode = room, AirTemperature = 0f });
            _em.SetComponentData(duct, new DuctStateComponent { TotalResistance = 1f });

            // Потока нет (вентилятора нет)

            // ТЭН выдает 10 кВт в стоячий воздух
            var heater = _em.CreateEntity(typeof(ElectricHeaterComponent));
            _em.SetComponentData(heater, new ElectricHeaterComponent { TargetDuct = duct, CurrentHeatOutput = 10f });

            // Запускаем ровно на 100 тиков (2 виртуальные секунды)
            RunSimulation(100);

            var ductData = _em.GetComponentData<AirDuctComponent>(duct);

            // ФОРМУЛА из кода: Temp += (HeatKW * 50f) * dt
            // Нагрев за 1 тик: 10 * 50 * 0.02 = 10 градусов. 
            // За 100 тиков температура должна вырасти ровно на 1000 градусов.
            Assert.AreEqual(0f, ductData.CurrentFlowRate, "Поток должен быть равен 0");
            Assert.AreEqual(1000f, ductData.AirTemperature, 0.1f, "Воздух не раскалился до расчетной аварийной температуры!");
        }

        // =========================================================================
        // ТЕСТ 3: ЕСТЕСТВЕННОЕ ОСТЫВАНИЕ ТРУБЫ ПРИ ВЫКЛЮЧЕННОМ ТЭНЕ
        // =========================================================================
        [Test]
        public void Heater_TurnedOff_WithNoFlow_ShouldCoolDownSlowly()
        {
            // Улица 20 °C (к ней будем остывать)
            var street = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(street, new AirNodeComponent { IsInfinite = true, Temperature = 20f });

            var duct = _em.CreateEntity(typeof(AirDuctComponent), typeof(DuctStateComponent));
            // Искусственно задаем стартовую температуру трубы в 100 °C
            _em.SetComponentData(duct, new AirDuctComponent { SourceNode = street, AirTemperature = 100f });
            _em.SetComponentData(duct, new DuctStateComponent { TotalHeatKW = 0f }); // ТЭН ВЫКЛЮЧЕН

            // Запускаем ровно на 1000 тиков (20 секунд)
            RunSimulation(1000);

            var ductData = _em.GetComponentData<AirDuctComponent>(duct);

            // МАТЕМАТИКА ОСТЫВАНИЯ:
            // Lerp multiplier: dt(0.02) * 0.05 = 0.001
            // Формула: T_new = T_old + (20 - T_old) * 0.001
            // Экспоненциальное затухание за 1000 шагов опустит температуру со 100 до ~49.4 °C.

            UnityEngine.Debug.Log($"Температура остывшей трубы: {ductData.AirTemperature:F1} °C");

            Assert.IsTrue(ductData.AirTemperature < 100f, "Труба вообще не остывает!");
            Assert.IsTrue(ductData.AirTemperature > 20f, "Труба остыла слишком быстро!");
            Assert.AreEqual(49.4f, ductData.AirTemperature, 0.5f, "Кривая остывания нарушена!");
        }
    }
}