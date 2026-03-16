using NUnit.Framework;
using SimBridge.Core.Time;
using SimVent.Components;
using SimVent.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace SimVent.Tests
{
    public class ThermodynamicsBenchmarkTest
    {
        private World _world;
        private EntityManager _em;
        private SystemHandle _physicsSystemHandle;

        // Выносим создание мира в Setup, чтобы не дублировать код в каждом тесте
        [SetUp]
        public void Setup()
        {
            _world = new World("PhysicsTestWorld");
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
            {
                _physicsSystemHandle.Update(_world.Unmanaged);
            }
        }

        // =========================================================================
        // ТЕСТ 1: БАЗОВОЕ ОХЛАЖДЕНИЕ КОМНАТЫ (Ваш исправленный тест)
        // =========================================================================
        [Test]
        public void RoomCoolingBenchmark_ShouldMatchPhysicsLaws()
        {
            var street = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(street, new AirNodeComponent { IsInfinite = true, Temperature = -15f, Pressure = 0f });

            var room = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(room, new AirNodeComponent { IsInfinite = false, Temperature = 20f, Volume = 100f, LeakFactor = 0.5f, TargetStreetTemp = -15f });

            var duct = _em.CreateEntity(typeof(AirDuctComponent), typeof(DuctStateComponent));
            _em.SetComponentData(duct, new AirDuctComponent { SourceNode = street, TargetNode = room, AirTemperature = -15f });
            _em.SetComponentData(duct, new DuctStateComponent { TotalResistance = 0.5f });

            var fan = _em.CreateEntity(typeof(FanComponent));
            _em.SetComponentData(fan, new FanComponent { TargetDuct = duct, CurrentSpeed = 1f, MaxPressure = 500f });

            RunSimulation(40000); // 13.3 минуты
                             
            var ductData = _em.GetComponentData<AirDuctComponent>(duct);
            var roomData = _em.GetComponentData<AirNodeComponent>(room);

            // ИСПРАВЛЕННЫЕ ОЖИДАНИЯ: 
            // Поток теперь стабилизируется на ~900 м³/ч
            Assert.IsTrue(math.abs(ductData.CurrentFlowRate - 900.9f) < 5f, $"Поток неверный: {ductData.CurrentFlowRate}");

            // Температура за счет меньшего потока остынет чуть-чуть медленнее (около -9.5°C)
            Assert.IsTrue(math.abs(roomData.Temperature - (-9.5f)) < 1.0f, $"Температура неверная: {roomData.Temperature}");

            RunSimulation(80000); // Доводим до 40 минут

            roomData = _em.GetComponentData<AirNodeComponent>(room);
            Assert.AreEqual(-15.0f, roomData.Temperature, 0.2f, "Температура не остыла до уличной!");
        }

        // =========================================================================
        // ТЕСТ 2: ТРУБА В ТРУБУ (Закон сохранения массы)
        // =========================================================================
        [Test]
        public void SeriesDucts_FlowContinuity_ShouldMatch()
        {
            var street = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(street, new AirNodeComponent { IsInfinite = true, Pressure = 0f });

            // Узел-тройник (V=10 - золотая середина для стабильности)
            var middleNode = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(middleNode, new AirNodeComponent { Volume = 10f, LeakFactor = 0f });

            // Труба 1 (Нагнетание)
            var duct1 = _em.CreateEntity(typeof(AirDuctComponent), typeof(DuctStateComponent));
            _em.SetComponentData(duct1, new AirDuctComponent { SourceNode = street, TargetNode = middleNode });
            _em.SetComponentData(duct1, new DuctStateComponent { TotalResistance = 0.5f });

            var fan = _em.CreateEntity(typeof(FanComponent));
            _em.SetComponentData(fan, new FanComponent { TargetDuct = duct1, CurrentSpeed = 1f, MaxPressure = 500f });

            // Труба 2 (Сброс обратно на улицу)
            var duct2 = _em.CreateEntity(typeof(AirDuctComponent), typeof(DuctStateComponent));
            _em.SetComponentData(duct2, new AirDuctComponent { SourceNode = middleNode, TargetNode = street });
            _em.SetComponentData(duct2, new DuctStateComponent { TotalResistance = 0.5f });

            // Ждем 5 минут (15 000 тиков) - этого хватит для V=10
            RunSimulation(15000);

            var d1Flow = _em.GetComponentData<AirDuctComponent>(duct1).CurrentFlowRate;
            var d2Flow = _em.GetComponentData<AirDuctComponent>(duct2).CurrentFlowRate;
            var midPressure = _em.GetComponentData<AirNodeComponent>(middleNode).Pressure;

            UnityEngine.Debug.Log($"[Series Test] P_mid: {midPressure:F1} Pa, Q1: {d1Flow:F1}, Q2: {d2Flow:F1}");

            // Теоретически: P_mid должно быть 250 Па (ровно половина напора), 
            // а поток Q = (500 - 250) / 0.5 = 500 м3/ч.
            Assert.AreEqual(d1Flow, d2Flow, 2.0f, "Воздух теряется между трубами!");
            Assert.AreEqual(500f, d1Flow, 10.0f, "Общий поток системы не соответствует сопротивлению!");
        }

        // =========================================================================
        // ТЕСТ 3: ПРИТОК И ВЫТЯЖКА (Баланс давлений)
        // =========================================================================
        [Test]
        public void BalancedSupplyAndExhaust_ShouldKeepZeroPressure()
        {
            var street = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(street, new AirNodeComponent { IsInfinite = true, Pressure = 0f });

            var room = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(room, new AirNodeComponent { Volume = 100f, LeakFactor = 0f }); // Абсолютно герметичная комната

            // П1: Приток (С улицы в комнату)
            var supplyDuct = _em.CreateEntity(typeof(AirDuctComponent), typeof(DuctStateComponent));
            _em.SetComponentData(supplyDuct, new AirDuctComponent { SourceNode = street, TargetNode = room });
            _em.SetComponentData(supplyDuct, new DuctStateComponent { TotalResistance = 0.5f });

            var supplyFan = _em.CreateEntity(typeof(FanComponent));
            _em.SetComponentData(supplyFan, new FanComponent { TargetDuct = supplyDuct, CurrentSpeed = 1f, MaxPressure = 500f });

            // В1: Вытяжка (Из комнаты на улицу)
            var exhaustDuct = _em.CreateEntity(typeof(AirDuctComponent), typeof(DuctStateComponent));
            _em.SetComponentData(exhaustDuct, new AirDuctComponent { SourceNode = room, TargetNode = street });
            _em.SetComponentData(exhaustDuct, new DuctStateComponent { TotalResistance = 0.5f });

            var exhaustFan = _em.CreateEntity(typeof(FanComponent));
            _em.SetComponentData(exhaustFan, new FanComponent { TargetDuct = exhaustDuct, CurrentSpeed = 1f, MaxPressure = 500f });

            RunSimulation(2000);

            var roomPressure = _em.GetComponentData<AirNodeComponent>(room).Pressure;
            var supplyFlow = _em.GetComponentData<AirDuctComponent>(supplyDuct).CurrentFlowRate;

            // Давление должно остаться около 0, так как вытяжка забирает ровно столько же, сколько дает приток
            Assert.AreEqual(0f, roomPressure, 5.0f, "Приток и вытяжка не сбалансированы!");
            // Поток должен быть большим (вентиляторы работают вхолостую без сопротивления давления комнаты)
            Assert.IsTrue(supplyFlow > 800f, "Поток слишком маленький для сбалансированной системы");
        }

        // =========================================================================
        // ТЕСТ 4: ЗАКРЫТАЯ ЗАСЛОНКА (Аварийный перегрев)
        // =========================================================================
        [Test]
        public void ClosedDamper_ShouldStopFlow_And_HeaterShouldOverheat()
        {
            var street = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(street, new AirNodeComponent { IsInfinite = true, Temperature = 0f });

            var room = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(room, new AirNodeComponent { Volume = 100f });

            var duct = _em.CreateEntity(typeof(AirDuctComponent), typeof(DuctStateComponent));
            _em.SetComponentData(duct, new AirDuctComponent { SourceNode = street, TargetNode = room, AirTemperature = 0f });
            _em.SetComponentData(duct, new DuctStateComponent { TotalResistance = 0.5f });

            // Вентилятор пытается дуть
            var fan = _em.CreateEntity(typeof(FanComponent));
            _em.SetComponentData(fan, new FanComponent { TargetDuct = duct, CurrentSpeed = 1f, MaxPressure = 500f });

            // Заслонка ЗАКРЫТА (0.01)
            var damper = _em.CreateEntity(typeof(DamperComponent));
            _em.SetComponentData(damper, new DamperComponent { TargetDuct = duct, CurrentOpening = 0.01f });

            // ТЭН шпарит на 15 кВт
            var heater = _em.CreateEntity(typeof(ElectricHeaterComponent));
            _em.SetComponentData(heater, new ElectricHeaterComponent { TargetDuct = duct, CurrentHeatOutput = 15f });

            RunSimulation(500); // 10 секунд симуляции

            var ductData = _em.GetComponentData<AirDuctComponent>(duct);

            // Поток должен быть практически нулевым из-за заслонки
            Assert.IsTrue(ductData.CurrentFlowRate < 15f, $"Заслонка не удержала поток! Поток: {ductData.CurrentFlowRate}");

            // Температура стоячего воздуха должна улететь в космос (сотни градусов за 10 сек)
            Assert.IsTrue(ductData.AirTemperature > 100f, $"Воздух не перегрелся! Темп: {ductData.AirTemperature}");
        }
    }
}