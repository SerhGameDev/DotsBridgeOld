using Unity.Entities;

namespace SimElectric
{
    public struct PushButtonTrigger : IComponentData
    {
        public Entity TargetButton;
    }
}