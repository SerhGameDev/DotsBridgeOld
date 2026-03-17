using Unity.Entities;

namespace DotsBridge
{
    /// <summary>
    /// Компонент-маркер для отслеживания жизненного цикла соединения.
    /// ICleanupComponentData гарантирует, что сущность не будет удалена, пока мы не снимем этот компонент.
    /// </summary>
    public struct BridgeClientCleanup : ICleanupComponentData
    {
        public int NetworkId;
    }
    /// <summary>
    /// Компонент для привязки сущности к конкретному клиенту (NetworkId).
    /// </summary>
    public struct NetworkOwner : IComponentData
    {
        public int Value;
    }
}