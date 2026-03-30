using Unity.Entities;
using Unity.NetCode;
using SimOil.Equipment;

namespace SimOil.Network
{
    [GhostComponent]
    public struct NetSync_FluidMixture : IComponentData
    {
        // Для UI и манометров оставляем 2 знака после запятой
        [GhostField(Quantization = 100)] public float Pressure; 
        [GhostField(Quantization = 10)] public float Temperature; // 1 знак
        [GhostField(Quantization = 10)] public float TotalMass;

        // Для визуала 3D (цвета трубы/резервуара) нам не нужны все 8 фракций.
        // Достаточно передать преобладающую фракцию.
        [GhostField] public FractionType DominantFraction;
    }

    [GhostComponent]
    public struct NetSync_FluidLink : IComponentData
    {
        // Текущий поток для анимации стрелочек направления в трубах
        [GhostField(Quantization = 100)] public float FlowRate;
    }
}