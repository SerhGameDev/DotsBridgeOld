using Unity.Entities;
using Unity.Mathematics;

namespace SimElectric
{
    // Специальный тег для включения/выключения визуализации
    public struct EnableWireHighlightTag : IComponentData { }

    // Данные для визуализации провода
    public struct WireVisuals : IComponentData
    {
        public float4 ActiveColor;
        public float4 InactiveColor;
    }

    // Данные для лампочки (потребителя)
    public struct Lightbulb : IComponentData
    {
        public float ThresholdVoltage; // Напряжение, при котором лампа загорается
        public float4 OnColor;
        public float4 OffColor;
    }
}