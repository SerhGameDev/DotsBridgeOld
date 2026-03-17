using System;
using System.Collections.Generic;

namespace ModbusBridge
{
    public abstract class ModbusSignal
    {
        public ushort Address { get; protected set; }

        protected ModbusSignal(ushort address)
        {
            Address = address;
        }
    }
    public abstract class ModbusSignal<T> : ModbusSignal
    {
        protected T _currentValue;
        private bool _isInitialized = false; // Флаг для первого срабатывания
        private Action<T> _onChangedCallback;

        protected ModbusSignal(ushort address) : base(address) { }

        public ModbusSignal<T> OnChangeSubscribe(Action<T> callback)
        {
            _onChangedCallback += callback;
            return this;
        }

        public void SetValueFromNetwork(T newValue)
        {
            if (_isInitialized && EqualityComparer<T>.Default.Equals(_currentValue, newValue))
                return;

            _currentValue = newValue;
            _isInitialized = true;

            // Используем встроенный в сервис диспетчер
            ModbusService.EnqueueAction(() =>
            {
                _onChangedCallback?.Invoke(_currentValue);
            });
        }
    }

    // Конкретные реализации типов данных
    public class SignalInt : ModbusSignal<int>
    {
        public SignalInt(ushort address) : base(address) { }
    }

    public class SignalBool : ModbusSignal<bool>
    {
        public SignalBool(ushort address) : base(address) { }
    }
    public class SignalFloat : ModbusSignal<float>
    {
        public SignalFloat(ushort address) : base(address) { }
    }
}