using Unity.Collections;
using Unity.Entities;


namespace DotsBridge
{
    public static partial class Dots
    {
        public static void Kill(this Entity entity)
        {
            if (Manager.Exists(entity))
            {
                Manager.DestroyEntity(entity);
            }
        }

        public static void Kill(NativeArray<Entity> entities)
        {
            if (entities.Length == 0) return;
            Manager.DestroyEntity(entities);
        }

        public static void Kill(NativeList<Entity> entities)
        {
            if (entities.Length == 0) return;
            Manager.DestroyEntity(entities.AsArray());
        }

        public static void Add<T>(NativeArray<Entity> entities, T componentData) where T : unmanaged, IComponentData
        {
            if (entities.Length == 0) return;

            Manager.AddComponent<T>(entities);

            for (int i = 0; i < entities.Length; i++)
            {
                Manager.SetComponentData(entities[i], componentData);
            }
        }

        public static void Add<T>(NativeList<Entity> entities, T componentData) where T : unmanaged, IComponentData
        {
            Add(entities.AsArray(), componentData);
        }


        public static void Add<T>(NativeArray<Entity> entities) where T : unmanaged, IComponentData
        {
            if (entities.Length == 0) return;
            Manager.AddComponent<T>(entities);
        }

        public static void Add<T>(NativeList<Entity> entities) where T : unmanaged, IComponentData
        {
            if (entities.Length == 0) return;
            Manager.AddComponent<T>(entities.AsArray());
        }

        public static void Remove<T>(NativeArray<Entity> entities) where T : unmanaged, IComponentData
        {
            if (entities.Length == 0) return;
            Manager.RemoveComponent<T>(entities);
        }

        public static void Remove<T>(NativeList<Entity> entities) where T : unmanaged, IComponentData
        {
            if (entities.Length == 0) return;
            Manager.RemoveComponent<T>(entities.AsArray());
        }
    }
}
