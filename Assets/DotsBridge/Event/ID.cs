using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public struct ID : IComponentData
    {
        public int Value; 
    }
    public struct EntityIdMap : IComponentData
    {
        public NativeParallelMultiHashMap<int, Entity> Map;
    }

    public struct SpawnParams
    {
        public Entity Prefab;
        public string GroupID;    
        public Vector3 Position; 
        public Quaternion Rotation; 
        public Vector3 Scale; 
        public int Count;  

        public static SpawnParams Default => new SpawnParams
        {
            Position = Vector3.zero,
            Rotation = Quaternion.identity,
            Scale = Vector3.one,
            Count = 1,
            GroupID = ""
        };
    }
    public struct ZoneComponent : IComponentData
    {
        public int InstanceID;
    }
    public struct PrefabManagerTag : IComponentData { }

    public struct PrefabLink : IBufferElementData
    {
        public int IDHash;
        public Entity PrefabEntity;
    }
}
