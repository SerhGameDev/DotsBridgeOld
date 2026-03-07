using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        public static World World => World.DefaultGameObjectInjectionWorld;
        public static EntityManager Manager => World.EntityManager;

        public static BridgeContext Server => new BridgeContext(ServerRegistry);
        public static BridgeContext Client => new BridgeContext(ClientRegistry); 
        public static BridgeContext Local => new BridgeContext(ServerRegistry);
        internal static BridgeRegistry ServerRegistry;
        internal static BridgeRegistry ClientRegistry;

        // Словарь для хранения зарегистрированных команд
        private static readonly Dictionary<int, DotsCommand> _commandRegistry = new Dictionary<int, DotsCommand>();

        public static int GetHash(string id) => new FixedString32Bytes(id).GetHashCode();
        // Вспомогательный метод для определения "кто сейчас главный"
        private static BridgeRegistry GetActiveRegistry()
        {
            if (ServerRegistry != null) return ServerRegistry;
            if (ClientRegistry != null) return ClientRegistry;
            return null;
        }

        /// <summary>
        /// Сбрасывает кэш для всех миров (например, при смене сцены)
        /// </summary>
        public static void ClearPrefabCache()
        {
            if (ServerRegistry != null) ServerRegistry.IsPrefabBufferCached = false;
            if (ClientRegistry != null) ClientRegistry.IsPrefabBufferCached = false;
        }

        /// <summary>
        /// Выполняет заранее зарегистрированную команду по её имени.
        /// Скорость: Максимальная (0 аллокаций памяти).
        /// </summary>
        public static void Execute(string commandName)
        {
            int hash = GetHash(commandName);
            if (_commandRegistry.TryGetValue(hash, out var command))
            {
                command.Execute();
            }
            else
            {
                Debug.LogError($"[DotsBridge] Попытка выполнить незарегистрированную команду '{commandName}'! Сначала вызовите .Register() у команды.");
            }
        }

        // Внутренний метод для сохранения команды
        internal static void RegisterCommandInternal(DotsCommand command)
        {
            int hash = GetHash(command.Name);
            if (_commandRegistry.ContainsKey(hash))
            {
                Debug.LogWarning($"[DotsBridge] Команда '{command.Name}' уже существует. Перезаписываем.");
            }
            _commandRegistry[hash] = command;
        }

        /// <summary>
        /// Очищает реестр команд (удобно вызывать при смене сцены).
        /// </summary>
        public static void ClearCommands()
        {
            _commandRegistry.Clear();
        }
    }
}
