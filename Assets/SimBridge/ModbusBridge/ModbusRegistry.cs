using System.Collections.Generic;
using System.Linq;

namespace ModbusBridge
{
    public class ModbusRegistry
    {
        private readonly Dictionary<ushort, SignalInt> _inputRegisters = new Dictionary<ushort, SignalInt>();
        private readonly Dictionary<ushort, SignalBool> _inputCoils = new Dictionary<ushort, SignalBool>();
        private readonly Dictionary<ushort, SignalFloat> _inputFloats = new Dictionary<ushort, SignalFloat>();

        // Фабричные методы
        public SignalInt GetOrCreateInt(ushort address)
        {
            if (!_inputRegisters.ContainsKey(address)) _inputRegisters[address] = new SignalInt(address);
            return _inputRegisters[address];
        }

        public SignalBool GetOrCreateBool(ushort address)
        {
            if (!_inputCoils.ContainsKey(address)) _inputCoils[address] = new SignalBool(address);
            return _inputCoils[address];
        }

        public SignalFloat GetOrCreateFloat(ushort address)
        {
            if (!_inputFloats.ContainsKey(address)) _inputFloats[address] = new SignalFloat(address);
            return _inputFloats[address];
        }

        // Высчитывает, сколько регистров нам нужно запросить по сети
        public bool TryGetPollingRange(out ushort minReg, out ushort countReg)
        {
            List<ushort> allAddresses = new List<ushort>();
            allAddresses.AddRange(_inputRegisters.Keys);
            allAddresses.AddRange(_inputCoils.Keys);

            foreach (var floatAddr in _inputFloats.Keys)
            {
                allAddresses.Add(floatAddr);
                allAddresses.Add((ushort)(floatAddr + 1));
            }

            if (allAddresses.Count == 0)
            {
                minReg = 0; countReg = 0;
                return false;
            }

            minReg = allAddresses.Min();
            countReg = (ushort)(allAddresses.Max() - minReg + 1);
            return true;
        }

        // Универсальный метод парсинга (вызывается из сетевого потока)
        public void ParseReceivedRegisters(ushort[] registers, ushort minReg, ModbusByteOrder floatOrder)
        {
            // 1. Парсинг Int
            foreach (var kvp in _inputRegisters)
            {
                int networkValue = registers[kvp.Key - minReg];
                kvp.Value.SetValueFromNetwork(networkValue);
            }

            // 2. Парсинг Bool (из Holding Registers)
            foreach (var kvp in _inputCoils)
            {
                bool networkValue = registers[kvp.Key - minReg] > 0;
                kvp.Value.SetValueFromNetwork(networkValue);
            }

            // 3. Парсинг Float
            foreach (var kvp in _inputFloats)
            {
                int localIndex = kvp.Key - minReg;
                if (localIndex + 1 < registers.Length)
                {
                    ushort reg1 = registers[localIndex];
                    ushort reg2 = registers[localIndex + 1];

                    float networkValue = ModbusDataConverter.ConvertRegistersToFloat(reg1, reg2, floatOrder);
                    kvp.Value.SetValueFromNetwork(networkValue);
                }
            }
        }
    }
}