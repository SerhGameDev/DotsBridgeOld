using Unity.Entities;

namespace SimOil.Network
{
    // Компонент, указывающий, из какой серверной сущности брать данные
    public struct SimTargetLink : IComponentData
    {
        public Entity TargetEntity;
    }
}