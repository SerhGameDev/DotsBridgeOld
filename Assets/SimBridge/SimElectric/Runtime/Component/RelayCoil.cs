using Unity.Entities;
using UnityEngine;

namespace SimElectric
{
    // Компонент катушки реле
    public struct RelayCoil : IComponentData
    {
        public Entity NodeA1;            // Пин A1
        public Entity NodeA2;            // Пин A2
        public float NominalVoltage;     // Номинальное напряжение (например, 24V)
        public float EnergizeThreshold;  // Порог срабатывания (обычно 70-80% от номинала)
        public bool IsEnergized;         // Текущее состояние (включено/выключено)
    }

    public enum ContactType
    {
        NormallyOpen,   // NO (Нормально разомкнутый) - проводит, когда катушка включена
        NormallyClosed  // NC (Нормально замкнутый) - проводит, когда катушка выключена
    }

    // Компонент контакта. Вешается на ту же сущность, что и Wire!
    public struct RelayContact : IComponentData
    {
        public Entity TargetCoil;        // Ссылка на сущность катушки, которая управляет этим контактом
        public ContactType Type;         // Тип контакта
    }
}