using SimElectric;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace SimElectric
{
    // Помещаем систему в группу инициализации
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct GraphInitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Система не будет работать, пока в мире не появятся сущности проводов
            state.RequireForUpdate<Wire>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Отключаем систему сразу после старта, чтобы она не пересобирала граф каждый кадр.
            // Мы включим ее вручную (state.Enabled = true), если пользователь добавит новый провод.
            state.Enabled = false;

            // Получаем доступ к буферам узлов. Важно строго соблюдать типы, чтобы не получить mismatch error.
            BufferLookup<ConnectedWireBuffer> connectedWireBuffers = SystemAPI.GetBufferLookup<ConnectedWireBuffer>(isReadOnly: false);

            // Сначала очищаем все буферы, чтобы при перестроении графа не дублировать связи
            foreach (var (buffer, entity) in SystemAPI.Query<DynamicBuffer<ConnectedWireBuffer>>().WithEntityAccess())
            {
                buffer.Clear();
            }

            // Проходим по всем проводам и линкуем их к узлам
            foreach (var (wire, wireEntity) in SystemAPI.Query<RefRO<Wire>>().WithEntityAccess())
            {
                // Проверяем, существуют ли узлы и есть ли у них нужный буфер
                if (connectedWireBuffers.HasBuffer(wire.ValueRO.NodeA))
                {
                    var bufferA = connectedWireBuffers[wire.ValueRO.NodeA];
                    bufferA.Add(new ConnectedWireBuffer { WireEntity = wireEntity });
                }

                if (connectedWireBuffers.HasBuffer(wire.ValueRO.NodeB))
                {
                    var bufferB = connectedWireBuffers[wire.ValueRO.NodeB];
                    bufferB.Add(new ConnectedWireBuffer { WireEntity = wireEntity });
                }
            }
        }
    }
}