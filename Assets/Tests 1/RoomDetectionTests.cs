using System.Collections;
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;
using DotsBridge;
using DotsBridge.Authoring;
using DotsBridge.Build;
using Unity.Transforms;

namespace DotsBridge.Tests
{
    public class RoomDetectionTests
    {
        private GameObject _bootstrapperGo;
        private GameObject _prefabContainerGo;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            _bootstrapperGo = new GameObject("Bootstrapper");
            _bootstrapperGo.AddComponent<DotsBridgeBootstrapper>();
    
            // Даем время на инициализацию систем
            yield return null; 
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            // Используем DestroyImmediate для тестов
            if (_bootstrapperGo != null) Object.DestroyImmediate(_bootstrapperGo);
            if (_prefabContainerGo != null) Object.DestroyImmediate(_prefabContainerGo);
    
            EntityBridge.DisposeAllStates();
            yield return null;
        }
        [UnityTest]
        public IEnumerator RoomDetection_CreatesOneRoom_WhenFourWallsFormSquare()
        {
            var world = EntityBridge.InSharedWorld();

            // ИСПРАВЛЕНИЕ: Добавляем typeof(Prefab), чтобы сущность игнорировалась запросами!
            var dummyWallPrefab = world.Manager.CreateEntity(
                typeof(Prefab), 
                typeof(LocalTransform), 
                typeof(WallTag), 
                typeof(WallComponent),
                typeof(ConnectionElement),
                typeof(DotsEntityAuthoring) 
            );
            world.Prefabs[EntityBridge.GetHash("TestWall")] = dummyWallPrefab;
            world.IsPrefabBufferCached = true;

            world.InitializeGrid(1f);

            // Строим квадрат
            world.BuildWall("TestWall", new float3(0,0,0), new float3(1,0,0)).Execute();
            world.BuildWall("TestWall", new float3(1,0,0), new float3(1,0,1)).Execute();
            world.BuildWall("TestWall", new float3(1,0,1), new float3(0,0,1)).Execute();
            world.BuildWall("TestWall", new float3(0,0,1), new float3(0,0,0)).Execute();

            // Триггер пересчета
            world.Manager.CreateEntity(typeof(TopologyChangedTag));

            // Прогоняем ECS конвейер
            for (int i = 0; i < 10; i++) 
            {
                world.World.Update();
                yield return null;
            }

            var roomQuery = world.Manager.CreateEntityQuery(typeof(RoomTag), typeof(RoomData));
            int roomCount = roomQuery.CalculateEntityCount();

            if(roomCount > 0)
            {
                var data = roomQuery.GetSingleton<RoomData>();
                Debug.Log($"[Test] Room Found! Area: {data.Area}, Perimeter: {data.Perimeter}");
            }

            Assert.AreEqual(1, roomCount, "Комната не создана. Буфер команд (ECB) не выполнился или граф не замкнут.");
        }
    }
}