using Unity.Entities;

namespace DotsBridge
{
    /// <summary>
    /// Элемент буфера, хранящий хэш имени и ссылку на запеченный Entity префаб.
    /// </summary>
    [InternalBufferCapacity(32)] // Предварительно выделяем память под 32 префаба
    public struct PrefabRegistryElement : IBufferElementData
    {
        public int NameHash;
        public Entity PrefabEntity;
    }
}