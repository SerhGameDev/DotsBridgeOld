using DotsBridge;
using SimOil;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public static class OilWorldGenerator
{
    public static void CreateSimpleChain(Entity nodePrefab, Entity linkPrefab)
    {
        var em = ServerBridge.Manager;
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // 1. Создаем узлы
        var nodeA = ecb.Instantiate(nodePrefab);
        ecb.SetComponent(nodeA, new FluidMixture { TotalMass = 1000f, Pressure = 2f });

        var nodeB = ecb.Instantiate(nodePrefab);
        ecb.SetComponent(nodeB, new FluidMixture { TotalMass = 0f, Pressure = 0.1f });

        // 2. Создаем трубу и ЯВНО связываем их
        var link = ecb.Instantiate(linkPrefab);
        ecb.SetComponent(link, new FluidLink { 
            NodeA = nodeA, 
            NodeB = nodeB, 
            CrossSectionArea = 0.05f 
        });

        ecb.Playback(em);
        ecb.Dispose();
        
        Debug.Log("[Server] Цепочка нефти создана динамически!");
    }
}