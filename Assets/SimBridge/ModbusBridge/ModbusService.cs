using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ModbusBridge
{
    public static class ModbusService
    {
        private static readonly Dictionary<string, ModbusConnection> _connections = new Dictionary<string, ModbusConnection>();
        private static readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();

        // Этот метод будет вызываться из MonoBehaviour-драйвера каждый кадр
        public static void UpdateMainThreadQueue()
        {
            while (_mainThreadQueue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        public static void EnqueueAction(Action action)
        {
            _mainThreadQueue.Enqueue(action);
        }

        // Билдеру больше не нужно передавать ссылку на сервис, так как сервис статический
        public static ModbusConnectionBuilder CreateConnection(string connectionId)
        {
            return new ModbusConnectionBuilder(connectionId);
        }

        public static ModbusConnection AddConnection(ModbusConnectionSettings settings)
        {
            if (_connections.ContainsKey(settings.ConnectionId))
                return _connections[settings.ConnectionId];

            var connection = new ModbusConnection(settings);
            _connections.Add(settings.ConnectionId, connection);
            return connection;
        }

        public static ModbusConnection GetConnection(string connectionId)
        {
            return _connections.GetValueOrDefault(connectionId);
        }

        public static void ActivateAll()
        {
            foreach (var c in _connections.Values) c.Activate();
        }

        public static void DeactivateAll()
        {
            foreach (var c in _connections.Values) c.Deactivate();
        }

        // Полезно вызывать при смене сцен или перезапуске симуляции
        public static void ClearAll()
        {
            DeactivateAll();
            _connections.Clear();
            // Очищаем очередь, чтобы старые коллбэки не выстрелили в новой сцене
            while (_mainThreadQueue.TryDequeue(out _)) { }
        }
    }

    public class ModbusConnectionSettings
    {
        public string ConnectionId { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; } = 502; // Стандартный порт по умолчанию
        public byte UnitId { get; set; } = 1; // ID по умолчанию
        public int ReadTimeout { get; set; } = 1000;
        public int WriteTimeout { get; set; } = 1000; 
        public ModbusByteOrder FloatOrder { get; set; } = ModbusByteOrder.CDAB;
    }
    public class ModbusConnectionBuilder
    {
        private ModbusConnectionSettings _settings;

        // Конструктор теперь принимает только ID
        public ModbusConnectionBuilder(string connectionId)
        {
            _settings = new ModbusConnectionSettings { ConnectionId = connectionId };
        }

        public ModbusConnectionBuilder SetIp(string ip) { _settings.IpAddress = ip; return this; }
        public ModbusConnectionBuilder SetPort(int port) { _settings.Port = port; return this; }
        public ModbusConnectionBuilder SetUnitId(byte unitId) { _settings.UnitId = unitId; return this; }
        public ModbusConnectionBuilder SetTimeouts(int read, int write)
        {
            _settings.ReadTimeout = read;
            _settings.WriteTimeout = write;
            return this;
        }
        public ModbusConnectionBuilder SetFloatOrder(ModbusByteOrder order)
        {
            _settings.FloatOrder = order;
            return this;
        }

        public ModbusConnection Build()
        {
            if (string.IsNullOrEmpty(_settings.IpAddress))
                throw new ArgumentException($"[Modbus] IP адрес не задан для {_settings.ConnectionId}");

            // Напрямую вызываем статический метод
            return ModbusService.AddConnection(_settings);
        }
    }
}