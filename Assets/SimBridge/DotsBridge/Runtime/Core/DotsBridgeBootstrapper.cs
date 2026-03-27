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
        // Массив для сверхбыстрого O(1) доступа вместо медленного Dictionary
        private static BridgeWorld[] _worldStates = new BridgeWorld[32];

        public static event Action<BridgeWorld> OnWorldCreated;
        public static event Action<BridgeWorld> OnWorldDestroyed;
        public static event Action OnServerStarted;
        public static event Action OnClientStarted;

        // O(1) доступ: идеально для вызова каждый кадр из MonoBehaviour
        public static BridgeWorld InCurrentWorld()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return null;

            ulong seq = world.SequenceNumber;
            
            // Если мир уже закэширован, мгновенно возвращаем его
            if (seq < (ulong)_worldStates.Length)
            {
                var bridge = _worldStates[seq];
                if (bridge != null) return bridge;
            }

            return GetOrCreateBridge(world);
        }

        internal static BridgeWorld GetOrCreateBridge(World targetWorld)
        {
            if (targetWorld == null || !targetWorld.IsCreated) return null;

            ulong seq = targetWorld.SequenceNumber;

            // Динамическое расширение массива, если SequenceNumber превысил длину
            if (seq >= (ulong)_worldStates.Length)
            {
                Array.Resize(ref _worldStates, Mathf.NextPowerOfTwo((int)seq + 1));
            }

            if (_worldStates[seq] == null)
            {
                var bridge = new BridgeWorld(targetWorld);
                _worldStates[seq] = bridge;

                // Инжектим систему очистки прямо в DOTS-мир
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

            return _worldStates[seq];
        }

        public static void HandleWorldDestroyed(World world)
        {
            ulong seq = world.SequenceNumber;
            if (seq < (ulong)_worldStates.Length && _worldStates[seq] != null)
            {
                var bridge = _worldStates[seq];
                OnWorldDestroyed?.Invoke(bridge);
                
                bridge.Dispose(); // Безопасная очистка NativeList
                _worldStates[seq] = null;
                
                Debug.Log($"[DotsBridge] Мир {world.Name} (Seq: {seq}) уничтожен. Мост очищен.");
            }
        }

        public static void DisposeAllStates()
        {
            for (int i = 0; i < _worldStates.Length; i++)
            {
                if (_worldStates[i] != null)
                {
                    _worldStates[i].Dispose();
                    _worldStates[i] = null;
                }
            }
        }

        // Вспомогательные методы пока оставляем, мы заменим их на строгие ссылки в Bootstrapper (Этап 2)
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
    }
}