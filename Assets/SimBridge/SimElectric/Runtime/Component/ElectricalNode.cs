using Unity.Entities;
using UnityEngine;

namespace SimElectric
{
    // Компонент узла (клеммы)
    public struct ElectricalNode : IComponentData
    {
        public float CurrentVoltage; // Текущий потенциал на клемме
        public float NextVoltage;    // Добавили буфер для записи
        public bool IsPowerSource;   // Является ли источником питания (например, клемма +24V)
        public float SourceVoltage;  // Если это источник, то сколько он выдает (24, 230)
        public bool IsGrounded;      // Является ли землей/нулем (0V)

        public bool CurrentHasGround;

        public float Resistance;       // Собственное сопротивление узла (Ом). Для лампы > 0.
        public float PathResistance;   // Накопленное сопротивление от источника (считается системой)
        public Entity ParentWire;      // "Хлебная крошка" - по какому проводу сюда пришел плюс
    }
}
