using Modbus.Device;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ModbusBridge
{
    public struct WriteCommand
    {
        public ushort Address;
        public ushort[] Registers;
    }
    public class ModbusConnection
    {
        public string ConnectionId => _settings.ConnectionId;

        private readonly ModbusConnectionSettings _settings;
        private readonly ModbusRegistry _signals = new ModbusRegistry();

        private TcpClient _client;
        private ModbusIpMaster _master;
        private volatile bool _isActive;

        public bool EnableDebug { get; set; } = false;
        public List<ModbusRegisterView> DebugRegisters { get; private set; } = new List<ModbusRegisterView>();
        private readonly ConcurrentQueue<WriteCommand> _writeQueue = new ConcurrentQueue<WriteCommand>();

        public ModbusConnection(ModbusConnectionSettings settings)
        {
            _settings = settings;
        }

        // Проксируем регистрацию сигналов в менеджер
        public SignalInt InputInt(ushort address) => _signals.GetOrCreateInt(address);
        public SignalBool InputBool(ushort address) => _signals.GetOrCreateBool(address);
        public SignalFloat InputFloat(ushort address) => _signals.GetOrCreateFloat(address);

        public void Activate()
        {
            if (_isActive) return;
            _isActive = true;
            Task.Run(() => PollingLoop());
            Debug.Log($"[Modbus] Соединение {ConnectionId} активировано.");
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        private void PollingLoop()
        {
            while (_isActive)
            {
                try
                {
                    _client = new TcpClient(_settings.IpAddress, _settings.Port);
                    _master = ModbusIpMaster.CreateIp(_client);
                    _master.Transport.ReadTimeout = _settings.ReadTimeout;
                    _master.Transport.WriteTimeout = _settings.WriteTimeout;

                    Debug.Log($"[Modbus] Успешное подключение к {_settings.IpAddress}");

                    // УБРАЛИ РАСЧЕТ ДИАПАЗОНА ОТСЮДА

                    while (_isActive && _client.Connected)
                    {
                        try
                        {
                            // === 1. ЗАПИСЬ ===
                            while (_writeQueue.TryDequeue(out WriteCommand cmd))
                            {
                                if (cmd.Registers.Length == 1)
                                    _master.WriteSingleRegister(_settings.UnitId, cmd.Address, cmd.Registers[0]);
                                else
                                    _master.WriteMultipleRegisters(_settings.UnitId, cmd.Address, cmd.Registers);
                            }

                            // === 2. РАСЧЕТ ДИАПАЗОНА ===
                            bool hasRegisters = _signals.TryGetPollingRange(out ushort minReg, out ushort countReg);

                            // === 3. ЧТЕНИЕ ===
                            if (hasRegisters)
                            {
                                ushort[] registers = _master.ReadHoldingRegisters(_settings.UnitId, minReg, countReg);
                                UpdateDebugView(registers, minReg);
                                _signals.ParseReceivedRegisters(registers, minReg, _settings.FloatOrder);
                            }
                        }
                        catch (Modbus.SlaveException slaveEx)
                        {
                            Debug.LogWarning($"[Modbus] ПЛК {_settings.IpAddress} отклонил запрос. Код ошибки: {slaveEx.SlaveExceptionCode}. Проверьте адреса в Unity и Quantity в симуляторе!");
                            Thread.Sleep(1000);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[Modbus] Физический разрыв связи с {_settings.IpAddress}: {e.Message}");
                            break; 
                        }

                        Thread.Sleep(50);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Modbus] Разрыв связи с {_settings.IpAddress}: {e.Message}");
                    Thread.Sleep(2000);
                }
                finally
                {
                    _master?.Dispose();
                    _client?.Close();
                }
            }
        }

        private void UpdateDebugView(ushort[] registers, ushort minReg)
        {
            if (EnableDebug)
            {
                DebugRegisters.Clear();
                for (int i = 0; i < registers.Length; i++)
                {
                    DebugRegisters.Add(new ModbusRegisterView { Address = (ushort)(minReg + i), Value = registers[i] });
                }
            }
            else if (DebugRegisters.Count > 0)
            {
                DebugRegisters.Clear();
            }
        }
        #region Методы для отправки данных (Write)

        // Запись 16-битного числа (1 регистр)
        public void WriteInt16(ushort address, short value)
        {
            _writeQueue.Enqueue(new WriteCommand
            {
                Address = address,
                Registers = new[] { ModbusDataConverter.ConvertInt16ToRegister(value) }
            });
        }

        // Запись 32-битного числа (2 регистра)
        public void WriteInt32(ushort address, int value)
        {
            _writeQueue.Enqueue(new WriteCommand
            {
                Address = address,
                Registers = ModbusDataConverter.ConvertInt32ToRegisters(value, _settings.FloatOrder)
            });
        }

        // Запись числа с плавающей точкой (2 регистра)
        public void WriteFloat(ushort address, float value)
        {
            _writeQueue.Enqueue(new WriteCommand
            {
                Address = address,
                Registers = ModbusDataConverter.ConvertFloatToRegisters(value, _settings.FloatOrder)
            });
        }

        // Запись булева значения в Holding Register (0 или 1)
        public void WriteBool(ushort address, bool value)
        {
            _writeQueue.Enqueue(new WriteCommand
            {
                Address = address,
                Registers = new[] { value ? (ushort)1 : (ushort)0 }
            });
        }

        #endregion
    }
}