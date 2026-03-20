using Unity.Entities;

namespace DotsBridge.Placement
{
    public enum RailAxis
    {
        X,
        Y,
        Z
    }

    public struct RailSurface : IComponentData
    {
        public RailAxis AllowedAxis;
    }
}
