using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DotsBridge
{
    public enum BridgeStartMode
    {
        Server,
        Client,
        ServerAndClient,
        Shared
    }
    [DefaultExecutionOrder(-100)]
    public class DotsBridgeBootstrapper : MonoBehaviour
    {
        [Header("Startup Configuration")]
        public BridgeStartMode Mode = BridgeStartMode.Shared;
        public bool AutoInitializeNetcode = true;

        private void Awake()
        {
            if(AutoInitializeNetcode)
                InitializeBridge();
        }

        private void InitializeBridge()
        {
            switch (Mode)
            {
                case BridgeStartMode.Server:
                    EntityBridge.InServerWorld();
                    break;
                case BridgeStartMode.Client:
                    EntityBridge.InClientWorld();
                    break;
                case BridgeStartMode.Shared:
                    EntityBridge.InServerWorld();
                    break;
            }

            Debug.Log($"[DotsBridge] Запуск в режиме {Mode}");
        }

        private void OnDestroy()
        {
            EntityBridge.DisposeAllStates();
        }
    }
    public static partial class EntityBridge
    {
        private static readonly Dictionary<World, BridgeWorld> _worldStates = new Dictionary<World, BridgeWorld>();

        public static event Action<BridgeWorld> OnWorldCreated;
        public static event Action<BridgeWorld> OnWorldDestroyed; 
        
        public static event Action OnServerStarted;
        public static event Action OnClientStarted;
        public static BridgeWorld InServerWorld()
        {
            foreach (var world in World.All)
            {
                if (world.IsServer()) return GetOrCreateBridge(world);
            }
            return null;
        }

        public static BridgeWorld InClientWorld()
        {
            foreach (var world in World.All)
            {
                if (world.IsClient()) return GetOrCreateBridge(world);
            }
            return null;
        }
        public static BridgeWorld InSharedWorld() => FindWorldByFlag(WorldFlags.Game);

        public static BridgeWorld InCurrentWorld() => GetOrCreateBridge(World.DefaultGameObjectInjectionWorld);

        public static BridgeWorld CreateWorld(string name, WorldFlags flags)
        {
            foreach (var w in World.All)
                if (w.Name == name) w.Dispose();

            var world = new World(name, flags);
            return GetOrCreateBridge(world);
        }

        private static BridgeWorld FindWorldByFlag(WorldFlags flag)
        {
            foreach (var world in World.All)
                if ((world.Flags & flag) != 0) return GetOrCreateBridge(world);
            return null;
        }
        internal static BridgeWorld GetOrCreateBridge(World targetWorld)
        {
            if (targetWorld == null || !targetWorld.IsCreated) return null;

            if (!_worldStates.TryGetValue(targetWorld, out var bridge))
            {
                bridge = new BridgeWorld(targetWorld);
                _worldStates.Add(targetWorld, bridge);

                targetWorld.GetOrCreateSystemManaged<BridgeCleanupSystem>();

                OnWorldCreated?.Invoke(bridge);

                if ((targetWorld.Flags & WorldFlags.GameServer) != 0)
                {
                    OnServerStarted?.Invoke();
                }
                else if ((targetWorld.Flags & WorldFlags.GameClient) != 0)
                {
                    OnClientStarted?.Invoke();
                }
            }
            return bridge;
        }

        public static void HandleWorldDestroyed(World world)
        {
            if (_worldStates.TryGetValue(world, out var bridge))
            {
                OnWorldDestroyed?.Invoke(bridge);
                bridge.Dispose(); 
                _worldStates.Remove(world);
                Debug.Log($"[DotsBridge] Мир {world.Name} уничтожен. Мост очищен.");
            }
        }
        public static Entity GetServerConnection(BridgeWorld bridgeWorld)
        {
            if (bridgeWorld == null) return Entity.Null;

            var query = bridgeWorld.Manager.CreateEntityQuery(typeof(NetworkId), typeof(NetworkStreamConnection));

            if (query.IsEmptyIgnoreFilter) return Entity.Null;

            using var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            return entities.Length > 0 ? entities[0] : Entity.Null;
        }
        public static ListEntity GetServerConnection(this ListEntity list, BridgeWorld bridgeWorld)
        {
            var query = bridgeWorld.Manager.CreateEntityQuery(typeof(NetworkId));
            var entity = new SingleEntity(query.IsEmptyIgnoreFilter ? Entity.Null : query.GetSingletonEntity(), bridgeWorld);
            return entity.ToListEntity();
        }
        public static void DisposeAllStates()
        {
            foreach (var state in _worldStates.Values) state.Dispose();
            _worldStates.Clear();
        }
    }
}