using Unity.Entities;

namespace SimVent
{
    public struct FanVisualLink : IComponentData
    {
        public Entity LogicalFan;    // Ссылка на сущность с FanComponent
        public float SpeedMultiplier; // Множитель для перевода логической скорости в визуальную
    }
}