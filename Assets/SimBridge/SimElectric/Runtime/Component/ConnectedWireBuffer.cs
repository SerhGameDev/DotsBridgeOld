using Unity.Entities;

namespace SimElectric
{
    // Буфер, который будет висеть на каждом Узле и хранить подключенные к нему провода
    [InternalBufferCapacity(4)] // Обычно в одну клемму вставляют 1-4 провода, это оптимизирует память
    public struct ConnectedWireBuffer : IBufferElementData
    {
        public Entity WireEntity;
    }
}
