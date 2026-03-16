using NUnit.Framework;
using SimBridge.Core.Time;
using SimVent.Components;
using SimVent.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace SimVent.Tests
{
    public class FilterPhysicsTest
    {
        private World _world;
        private EntityManager _em;
        private SystemHandle _physicsSystemHandle;

        [SetUp]
        public void Setup()
        {
            _world = new World("FilterTestWorld");
            _em = _world.EntityManager;
            _physicsSystemHandle = _world.CreateSystem<DuctPhysicsSystem>();
            var timeEntity = _em.CreateEntity(typeof(SimulationTimeComponent));
            _em.SetComponentData(timeEntity, new SimulationTimeComponent { FixedStep = 0.02f });
        }

        [TearDown]
        public void TearDown() => _world.Dispose();

        [Test]
        public void DirtyFilter_ShouldSignificantlyReduceFlow()
        {
            var street = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(street, new AirNodeComponent { IsInfinite = true });
            var room = _em.CreateEntity(typeof(AirNodeComponent));
            _em.SetComponentData(room, new AirNodeComponent { IsInfinite = true });

            var duct = _em.CreateEntity(typeof(AirDuctComponent), typeof(DuctStateComponent));
            _em.SetComponentData(duct, new AirDuctComponent { SourceNode = street, TargetNode = room });
            _em.SetComponentData(duct, new DuctStateComponent { TotalResistance = 0.5f });

            var fan = _em.CreateEntity(typeof(FanComponent));
            _em.SetComponentData(fan, new FanComponent { TargetDuct = duct, CurrentSpeed = 1f, MaxPressure = 500f });

            var filter = _em.CreateEntity(typeof(FilterComponent));
            _em.SetComponentData(filter, new FilterComponent
            {
                TargetDuct = duct,
                NominalResistance = 0.1f, // Чистый фильтр
                Dirtiness = 0f,
                MaxDirtinessResistance = 2.0f // Если забьется, станет очень "тугим"
            });

            // 1. ПРОВЕРКА С ЧИСТЫМ ФИЛЬТРОМ
            for (int i = 0; i < 500; i++) _physicsSystemHandle.Update(_world.Unmanaged);
            float cleanFlow = _em.GetComponentData<AirDuctComponent>(duct).CurrentFlowRate;
            UnityEngine.Debug.Log($"Поток с чистым фильтром: {cleanFlow:F1} м3/ч");

            // 2. ЗАБИВАЕМ ФИЛЬТР НА 100%
            _em.SetComponentData(filter, new FilterComponent
            {
                TargetDuct = duct,
                NominalResistance = 0.1f,
                Dirtiness = 1f,
                MaxDirtinessResistance = 2.0f
            });

            for (int i = 0; i < 500; i++) _physicsSystemHandle.Update(_world.Unmanaged);
            float dirtyFlow = _em.GetComponentData<AirDuctComponent>(duct).CurrentFlowRate;
            UnityEngine.Debug.Log($"Поток с грязным фильтром: {dirtyFlow:F1} м3/ч");

            // Ожидаем, что поток упадет минимум в 3 раза
            Assert.IsTrue(dirtyFlow < (cleanFlow / 3f), "Забитый фильтр не оказал влияния на поток воздуха!");
        }
    }
}