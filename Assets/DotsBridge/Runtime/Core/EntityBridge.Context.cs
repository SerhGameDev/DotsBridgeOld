using System;
using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        internal static BridgeState ServerState;
        internal static BridgeState ClientState;
        internal static BridgeState DefaultState;

        internal static BridgeState GetActiveState()
        {
            if (ServerState != null) return ServerState;
            if (ClientState != null) return ClientState;
            if (DefaultState != null) return DefaultState;

            return null;
        }

        // =========================================================
        // ЕДИНСТВЕННЫЕ ТОЧКИ ВХОДА (CQRS)
        // =========================================================

        /// <summary> Создает команду для изменения данных. </summary>
        public static DotsCommand Command(string commandName)
        {
            return new DotsCommand(commandName);
        }

        internal static Entity GetPrefabInternal(BridgeState state, string name)
        {
            if (state == null) return Entity.Null;
            int hash = GetHash(name);

            if (!state.IsPrefabBufferCached)
            {
                var query = state.Manager.CreateEntityQuery(typeof(PrefabRegistryElement));
                if (!query.IsEmptyIgnoreFilter)
                {
                    var buffer = query.GetSingletonBuffer<PrefabRegistryElement>(true);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        state.Prefabs[buffer[i].NameHash] = buffer[i].PrefabEntity;
                    }
                    state.IsPrefabBufferCached = true;
                }
                else
                {
                    UnityEngine.Debug.LogError($"[DotsBridge] В мире {state.World.Name} не найден контейнер префабов!");
                    return Entity.Null;
                }
            }

            if (state.Prefabs.TryGetValue(hash, out Entity prefab)) return prefab;

            UnityEngine.Debug.LogError($"[DotsBridge] Префаб '{name}' не найден в мире {state.World.Name}!");
            return Entity.Null;
        }

        internal static EntityBatch GetByIdInternal(BridgeState state, string id)
        {
            if (state == null) return default;
            int hash = GetHash(id);

            if (!state.Containers.TryGetValue(hash, out var container))
            {
                container = new EntityContainer(hash, state);
                state.Containers.Add(hash, container);
            }
            return container.GetBatch(); // Не забудь обновить GetBatch, чтобы он передавал State!
        }
    }
}