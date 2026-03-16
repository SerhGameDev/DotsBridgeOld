using Unity.Entities;

public struct FilterComponent : IComponentData
{
    public Entity TargetDuct;
    public float NominalResistance; // Сопротивление чистого фильтра (например, 0.2)
    public float Dirtiness;         // Степень загрязнения от 0 до 1
    public float MaxDirtinessResistance; // Добавочное сопротивление при 100% загрязнении
}