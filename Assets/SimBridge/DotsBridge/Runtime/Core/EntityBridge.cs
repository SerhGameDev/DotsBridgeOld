using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        private static readonly Dictionary<int, DotsCommand> _commandRegistry = new Dictionary<int, DotsCommand>();

        public static int GetHash(string id) => new FixedString32Bytes(id).GetHashCode();

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
