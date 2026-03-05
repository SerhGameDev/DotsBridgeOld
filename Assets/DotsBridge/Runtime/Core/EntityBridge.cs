using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        public static World World => World.DefaultGameObjectInjectionWorld;
        public static EntityManager Manager => World.EntityManager;

        public static readonly Dictionary<int, EntityContainer> Containers = new Dictionary<int, EntityContainer>();
        private static readonly Dictionary<string, ComponentType> TagRegistry = new Dictionary<string, ComponentType>();
        public static readonly Dictionary<Entity, Action<Entity>> OnDestroyEvents = new Dictionary<Entity, Action<Entity>>();
        
        public static int GetHash(string id) => new FixedString32Bytes(id).GetHashCode(); 

        public static void RegisterDestroy(Entity entity, Action<Entity> action)
        {
            if (OnDestroyEvents.ContainsKey(entity))
                OnDestroyEvents[entity] += action;
            else
                OnDestroyEvents[entity] = action;
        }
    }
}
