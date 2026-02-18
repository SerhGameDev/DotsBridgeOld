using DotsBridge.Spawning;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    public static partial class Dots
    {
        public static SpawnerBuilder Spawn()
        {
            return new SpawnerBuilder(Manager);
        }

        /// <summary>
         /// Поставить спаунер на паузу.
         /// </summary>
        public static void PauseSpawner(string id, bool pause)
        {
            var entities = Find(id, Allocator.Temp); 

            foreach (var e in entities)
            {
                if (Manager.HasComponent<SpawnRequest>(e))
                {
                    var req = Manager.GetComponentData<SpawnRequest>(e);
                    req.IsPaused = pause;
                    Manager.SetComponentData(e, req);
                }
            }
            entities.Dispose();
        }

        public static void StopSpawner(string id)
        {
            var entities = Find(id, Unity.Collections.Allocator.Temp);
            Manager.DestroyEntity(entities.AsArray());
            entities.Dispose();
        }
    }
}