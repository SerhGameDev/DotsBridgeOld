using Unity.Collections;
using Unity.Entities;

namespace SimOil.Tests
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ServerFluidSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Ищем наш конфиг со ссылками на префабы
            if (!SystemAPI.TryGetSingleton<FluidSpawnerConfig>(out var config)) return;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // 1. Спавним Резервуар А
            var nodeA = ecb.Instantiate(config.NodePrefab);
            ecb.SetComponent(nodeA, new FluidMixture { 
                TotalMass = 1000f, Pressure = 2.0f, Temperature = 50f, FractionMazut = 1f 
            });

            // 2. Спавним Резервуар Б
            var nodeB = ecb.Instantiate(config.NodePrefab);
            ecb.SetComponent(nodeB, new FluidMixture { 
                TotalMass = 10f, Pressure = 0.1f, Temperature = 20f, FractionLightNaphtha = 1f 
            });

            // 3. Спавним Трубу и соединяем их
            var link = ecb.Instantiate(config.LinkPrefab);
            ecb.SetComponent(link, new FluidLink { NodeA = nodeA, NodeB = nodeB, CrossSectionArea = 0.05f });
            ecb.SetComponent(link, new PumpData { MaxPressureBoost = 5.0f, CurrentPower = 1.0f });

            UnityEngine.Debug.Log("[ServerSpawn] Сеть труб успешно заспавнена на сервере!");

            // Удаляем конфиг, чтобы не спавнить трубы каждый кадр
            ecb.DestroyEntity(SystemAPI.GetSingletonEntity<FluidSpawnerConfig>());

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}