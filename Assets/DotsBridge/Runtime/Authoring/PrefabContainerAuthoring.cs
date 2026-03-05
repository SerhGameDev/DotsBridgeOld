using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public class PrefabContainerAuthoring : MonoBehaviour
    {
        [Serializable]
        public struct PrefabEntry
        {
            public string Name;  
            public GameObject Prefab; 
        }

        [Header("Registry")]
        public List<PrefabEntry> Prefabs = new List<PrefabEntry>();

        public class Baker : Baker<PrefabContainerAuthoring>
        {
            public override void Bake(PrefabContainerAuthoring authoring)
            {
                // Нам не нужен Transform для самого контейнера
                var entity = GetEntity(TransformUsageFlags.None);

                // Добавляем буфер к сущности
                var buffer = AddBuffer<PrefabRegistryElement>(entity);

                foreach (var entry in authoring.Prefabs)
                {
                    if (entry.Prefab == null) continue;

                    buffer.Add(new PrefabRegistryElement
                    {
                        NameHash = EntityBridge.GetHash(entry.Name),
                        PrefabEntity = GetEntity(entry.Prefab, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }
}