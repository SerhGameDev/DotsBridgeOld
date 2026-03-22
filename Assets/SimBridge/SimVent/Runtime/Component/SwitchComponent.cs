using Unity.Entities;

namespace SimElectric
{
    // Компонент, указывающий, что данный провод является ручным переключателем
    public struct SwitchComponent : IComponentData
    {
        public bool IsOn; // Целевое состояние: включен (true) или выключен (false)
    }
}