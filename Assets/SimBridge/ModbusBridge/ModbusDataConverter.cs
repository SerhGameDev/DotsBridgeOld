using System;

namespace ModbusBridge
{
    // Переименовали, так как порядок байт актуален для ЛЮБЫХ 32-битных данных (Float, Int32, UInt32)
    public enum ModbusByteOrder
    {
        ABCD, // Big Endian (Standard)
        CDAB, // Word Swap (Часто встречается в Siemens / Овен)
        BADC, // Byte Swap
        DCBA  // Little Endian
    }

    public static class ModbusDataConverter
    {
        #region ЧТЕНИЕ (2 регистра -> 32-битное значение)

        public static float ConvertRegistersToFloat(ushort reg1, ushort reg2, ModbusByteOrder byteOrder) =>
            BitConverter.ToSingle(GetOrderedBytes(reg1, reg2, byteOrder), 0);

        public static int ConvertRegistersToInt32(ushort reg1, ushort reg2, ModbusByteOrder byteOrder) =>
            BitConverter.ToInt32(GetOrderedBytes(reg1, reg2, byteOrder), 0);

        public static uint ConvertRegistersToUInt32(ushort reg1, ushort reg2, ModbusByteOrder byteOrder) =>
            BitConverter.ToUInt32(GetOrderedBytes(reg1, reg2, byteOrder), 0);

        // Вспомогательный метод: извлекает и сортирует 4 байта из двух регистров
        private static byte[] GetOrderedBytes(ushort reg1, ushort reg2, ModbusByteOrder byteOrder)
        {
            byte a = (byte)(reg1 >> 8); byte b = (byte)(reg1 & 0xFF);
            byte c = (byte)(reg2 >> 8); byte d = (byte)(reg2 & 0xFF);
            byte[] bytes = new byte[4];

            if (BitConverter.IsLittleEndian)
            {
                switch (byteOrder)
                {
                    case ModbusByteOrder.ABCD: bytes = new[] { d, c, b, a }; break;
                    case ModbusByteOrder.CDAB: bytes = new[] { b, a, d, c }; break;
                    case ModbusByteOrder.BADC: bytes = new[] { c, d, a, b }; break;
                    case ModbusByteOrder.DCBA: bytes = new[] { a, b, c, d }; break;
                }
            }
            else
            {
                switch (byteOrder)
                {
                    case ModbusByteOrder.ABCD: bytes = new[] { a, b, c, d }; break;
                    case ModbusByteOrder.CDAB: bytes = new[] { c, d, a, b }; break;
                    case ModbusByteOrder.BADC: bytes = new[] { b, a, d, c }; break;
                    case ModbusByteOrder.DCBA: bytes = new[] { d, c, b, a }; break;
                }
            }
            return bytes;
        }

        #endregion

        #region ЧТЕНИЕ (1 регистр -> 16-битное значение)

        // Чтение 16-битных значений не требует склейки, только приведение типов
        public static short ConvertRegisterToInt16(ushort reg) => (short)reg;
        public static ushort ConvertRegisterToUInt16(ushort reg) => reg;

        #endregion

        #region ЗАПИСЬ (32-битное значение -> 2 регистра)

        public static ushort[] ConvertFloatToRegisters(float value, ModbusByteOrder order) =>
            GetRegistersFromBytes(BitConverter.GetBytes(value), order);

        public static ushort[] ConvertInt32ToRegisters(int value, ModbusByteOrder order) =>
            GetRegistersFromBytes(BitConverter.GetBytes(value), order);

        public static ushort[] ConvertUInt32ToRegisters(uint value, ModbusByteOrder order) =>
            GetRegistersFromBytes(BitConverter.GetBytes(value), order);

        // Вспомогательный метод: берет 4 байта, сортирует и пакует в 2 регистра
        private static ushort[] GetRegistersFromBytes(byte[] bytes, ModbusByteOrder byteOrder)
        {
            byte a = 0, b = 0, c = 0, d = 0;

            if (BitConverter.IsLittleEndian)
            {
                switch (byteOrder)
                {
                    case ModbusByteOrder.ABCD: d = bytes[0]; c = bytes[1]; b = bytes[2]; a = bytes[3]; break;
                    case ModbusByteOrder.CDAB: b = bytes[0]; a = bytes[1]; d = bytes[2]; c = bytes[3]; break;
                    case ModbusByteOrder.BADC: c = bytes[0]; d = bytes[1]; a = bytes[2]; b = bytes[3]; break;
                    case ModbusByteOrder.DCBA: a = bytes[0]; b = bytes[1]; c = bytes[2]; d = bytes[3]; break;
                }
            }
            else
            {
                switch (byteOrder)
                {
                    case ModbusByteOrder.ABCD: a = bytes[0]; b = bytes[1]; c = bytes[2]; d = bytes[3]; break;
                    case ModbusByteOrder.CDAB: c = bytes[0]; d = bytes[1]; a = bytes[2]; b = bytes[3]; break;
                    case ModbusByteOrder.BADC: b = bytes[0]; a = bytes[1]; d = bytes[2]; c = bytes[3]; break;
                    case ModbusByteOrder.DCBA: d = bytes[0]; c = bytes[1]; b = bytes[2]; a = bytes[3]; break;
                }
            }

            ushort reg1 = (ushort)((a << 8) | b);
            ushort reg2 = (ushort)((c << 8) | d);

            return new ushort[] { reg1, reg2 };
        }

        #endregion

        #region ЗАПИСЬ (16-битное значение -> 1 регистр)

        public static ushort ConvertInt16ToRegister(short value) => (ushort)value;

        #endregion
    }
}