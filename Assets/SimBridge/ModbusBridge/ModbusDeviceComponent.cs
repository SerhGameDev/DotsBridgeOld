using ModbusBridge;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ModbusBridge
{
    public class ModbusDeviceComponent : MonoBehaviour
    {
        [Header("Настройки Подключения")]
        public string ConnectionId = "Cabinet_1";
        public string IpAddress = "127.0.0.1";
        public int Port = 502;
        public byte UnitId = 1;

        [Header("Чтение (Входящие данные)")]
        [Tooltip("Список сигналов для чтения из ПЛК")]
        public List<ModbusReadSignal> ReadSignals = new List<ModbusReadSignal>();

        [Header("Запись (Тестовая отправка)")]
        [Tooltip("Заполните список и нажмите 'Отправить тестовые команды' в контекстном меню скрипта (три точки)")]
        public List<ModbusWriteCommand> TestWriteCommands = new List<ModbusWriteCommand>();

        [Header("Отладка массивов (Только чтение)")]
        [Tooltip("Включить перехват и отрисовку сырых регистров? (Снижает производительность)")]
        public bool EnableDebugView = false;
        public List<ModbusRegisterView> DebugRegisters;

        [Header("Системные настройки")]
        public ModbusByteOrder ModbusByteOrder = ModbusByteOrder.CDAB;

        private ModbusConnection _connection;

        private void Start()
        {
            InitializeAndConnect();
        }

        private void Update()
        {
            if (_connection != null)
            {
                _connection.EnableDebug = EnableDebugView;

                if (EnableDebugView)
                {
                    DebugRegisters = _connection.DebugRegisters;
                }
                else if (DebugRegisters != null && DebugRegisters.Count > 0)
                {
                    DebugRegisters.Clear();
                }
            }
        }

        [ContextMenu("Переподключить (Reconnect)")]
        public void Reconnect()
        {
            Debug.Log($"[ModbusDevice] Перезапуск соединения {ConnectionId}...");
            if (_connection != null)
            {
                _connection.Deactivate();
            }
            InitializeAndConnect();
        }

        [ContextMenu("Отправить тестовые команды (Send Write Commands)")]
        public void SendTestCommands()
        {
            if (_connection == null)
            {
                Debug.LogWarning("[ModbusDevice] Нет активного подключения для отправки!");
                return;
            }

            foreach (var cmd in TestWriteCommands)
            {
                try
                {
                    switch (cmd.DataType)
                    {
                        case ModbusDataType.Int16:
                            if (short.TryParse(cmd.ValueStr, out short sVal))
                                _connection.WriteInt16(cmd.Address, sVal);
                            else Debug.LogError($"[Modbus] Ошибка парсинга Int16: {cmd.ValueStr}");
                            break;

                        case ModbusDataType.Int32:
                            if (int.TryParse(cmd.ValueStr, out int iVal))
                                _connection.WriteInt32(cmd.Address, iVal);
                            else Debug.LogError($"[Modbus] Ошибка парсинга Int32: {cmd.ValueStr}");
                            break;

                        case ModbusDataType.Float:
                            string fStr = cmd.ValueStr.Replace(',', '.');
                            if (float.TryParse(fStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float fVal))
                                _connection.WriteFloat(cmd.Address, fVal);
                            else Debug.LogError($"[Modbus] Ошибка парсинга Float: {cmd.ValueStr}");
                            break;

                        case ModbusDataType.Bool:
                            string bStr = cmd.ValueStr.ToLower();
                            if (bStr == "true" || bStr == "1") _connection.WriteBool(cmd.Address, true);
                            else if (bStr == "false" || bStr == "0") _connection.WriteBool(cmd.Address, false);
                            else Debug.LogError($"[Modbus] Ошибка парсинга Bool: {cmd.ValueStr}");
                            break;
                    }
                    Debug.Log($"[ModbusDevice] Запись в очередь -> Адрес: {cmd.Address} | Тип: {cmd.DataType} | Значение: {cmd.ValueStr}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ModbusDevice] Ошибка при отправке команды {cmd.Name}: {ex.Message}");
                }
            }
        }

        private void InitializeAndConnect()
        {
            ModbusService.CreateConnection(ConnectionId)
                .SetIp(IpAddress)
                .SetPort(Port)
                .SetUnitId(UnitId)
                .SetFloatOrder(ModbusByteOrder)
                .Build();

            _connection = ModbusService.GetConnection(ConnectionId);

            // Единый цикл подписки для всех типов сигналов
            foreach (var cfg in ReadSignals)
            {
                switch (cfg.DataType)
                {
                    case ModbusDataType.Int16:
                    case ModbusDataType.Int32:
                        _connection.InputInt(cfg.Address).OnChangeSubscribe(val =>
                        {
                            if (cfg.LogToConsole) Debug.Log($"<color=cyan>[Modbus In]</color> {cfg.Name} (Addr {cfg.Address}): {val}");
                            cfg.OnIntChanged?.Invoke(val);
                        });
                        break;

                    case ModbusDataType.Float:
                        _connection.InputFloat(cfg.Address).OnChangeSubscribe(val =>
                        {
                            if (cfg.LogToConsole) Debug.Log($"<color=cyan>[Modbus In]</color> {cfg.Name} (Addr {cfg.Address}): {val}");
                            cfg.OnFloatChanged?.Invoke(val);
                        });
                        break;

                    case ModbusDataType.Bool:
                        _connection.InputBool(cfg.Address).OnChangeSubscribe(val =>
                        {
                            if (cfg.LogToConsole) Debug.Log($"<color=cyan>[Modbus In]</color> {cfg.Name} (Addr {cfg.Address}): {val}");
                            cfg.OnBoolChanged?.Invoke(val);
                        });
                        break;
                }
            }

            _connection.Activate();
        }

        private void OnDestroy()
        {
            _connection?.Deactivate();
        }

        public void SendInt16(ushort address, short value) => _connection?.WriteInt16(address, value);
        public void SendInt32(ushort address, int value) => _connection?.WriteInt32(address, value);
        public void SendFloat(ushort address, float value) => _connection?.WriteFloat(address, value);
        public void SendBool(ushort address, bool value) => _connection?.WriteBool(address, value);
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ И СТРУКТУРЫ ---

    public enum ModbusDataType
    {
        Int16,
        Int32,
        Float,
        Bool
    }

    [Serializable]
    public class ModbusReadSignal
    {
        public string Name = "New Sensor";
        public ushort Address;
        public ModbusDataType DataType;

        [Tooltip("Выводить значение в консоль Unity при каждом изменении?")]
        public bool LogToConsole = false;

        // В инспекторе пользователь заполняет только тот Event, который соответствует DataType
        public ModbusIntEvent OnIntChanged;
        public ModbusFloatEvent OnFloatChanged;
        public ModbusBoolEvent OnBoolChanged;
    }

    [Serializable]
    public class ModbusWriteCommand
    {
        public string Name = "Test Command";
        public ushort Address;
        public ModbusDataType DataType;
        [Tooltip("Для Bool пишите true/false или 1/0. Для Float используйте точку (5.5)")]
        public string ValueStr = "0";
    }

    [Serializable]
    public struct ModbusRegisterView
    {
        public ushort Address;
        public ushort Value;
    }

    [Serializable] public class ModbusIntEvent : UnityEvent<int> { }
    [Serializable] public class ModbusFloatEvent : UnityEvent<float> { }
    [Serializable] public class ModbusBoolEvent : UnityEvent<bool> { }
}