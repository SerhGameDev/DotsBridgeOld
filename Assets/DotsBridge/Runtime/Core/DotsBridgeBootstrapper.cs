using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    [DefaultExecutionOrder(-100)] // Должен просыпаться раньше других скриптов
    public class DotsBridgeBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            // Опционально: делаем его неубиваемым при смене сцен, если мост глобальный
            // DontDestroyOnLoad(gameObject);

            // Прогреваем дефолтный мир при старте
            if (World.DefaultGameObjectInjectionWorld != null)
            {
                EntityBridge.GetState(World.DefaultGameObjectInjectionWorld);
            }
        }

        private void OnDestroy()
        {
            // Полная зачистка всех состояний моста (для всех миров)
            EntityBridge.DisposeAllStates();
        }
    }
    public static partial class EntityBridge
    {
        private static readonly Dictionary<World, BridgeState> _worldStates = new Dictionary<World, BridgeState>();

        // --- УПРАВЛЕНИЕ МИРАМИ ---

        public static World DefaultWorld => World.DefaultGameObjectInjectionWorld;

        public static BridgeState GetState(World world)
        {
            if (world == null || !world.IsCreated) return null;
            if (!_worldStates.TryGetValue(world, out var state))
            {
                state = new BridgeState(world);
                _worldStates[world] = state;
            }
            return state;
        }

        public static void DisposeAllStates()
        {
            foreach (var state in _worldStates.Values)
                state.Dispose();
            _worldStates.Clear();
        }

        // --- ТОЧКИ ВХОДА ДЛЯ МУЛЬТИПЛЕЕРА ---

        public static BridgeContext In(World world) => new BridgeContext(GetState(world));
        /// <summary>
        /// Автоматически находит серверный мир (совместимо с Netcode for Entities).
        /// </summary>
        public static BridgeContext InServerWorld()
        {
            foreach (var world in World.All)
            {
                if (world.IsCreated && world.Name.Contains("ServerWorld"))
                    return new BridgeContext(GetState(world));
            }

            UnityEngine.Debug.LogWarning("[DotsBridge] Серверный мир не найден! Возврат к DefaultWorld.");
            // Fallback: если сервера нет, возвращаем дефолтный мир, чтобы игра не крашилась
            return new BridgeContext(GetState(DefaultWorld));
        }

        /// <summary>
        /// Автоматически находит клиентский мир (совместимо с Netcode for Entities).
        /// </summary>
        public static BridgeContext InClientWorld()
        {
            foreach (var world in World.All)
            {
                if (world.IsCreated && world.Name.Contains("ClientWorld"))
                    return new BridgeContext(GetState(world));
            }

            UnityEngine.Debug.LogWarning("[DotsBridge] Клиентский мир не найден! Возврат к DefaultWorld.");
            return new BridgeContext(GetState(DefaultWorld));
        }

    }
}