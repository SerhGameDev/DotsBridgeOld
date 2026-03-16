using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SimElectric
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(RelayLogicSystem))]
    [BurstCompile]
    public partial struct CircuitSolverSystem : ISystem
    {
        private ComponentLookup<ElectricalNode> nodeLookup;
        private ComponentLookup<Wire> wireLookup; // ТЕПЕРЬ ОН БУДЕТ ЗАПИСЫВАТЬСЯ
        private BufferLookup<ConnectedWireBuffer> bufferLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationControl>();
            nodeLookup = state.GetComponentLookup<ElectricalNode>(isReadOnly: false);
            // ВАЖНО: теперь isReadOnly: false, так как мы будем писать туда Амперы и рвать провода
            wireLookup = state.GetComponentLookup<Wire>(isReadOnly: false);
            bufferLookup = state.GetBufferLookup<ConnectedWireBuffer>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var control = SystemAPI.GetSingleton<SimulationControl>();
            if (!control.IsRunning && !control.StepNextFrame) return;

            nodeLookup.Update(ref state);
            wireLookup.Update(ref state);
            bufferLookup.Update(ref state);

            var nodeQuery = SystemAPI.QueryBuilder().WithAll<ElectricalNode>().Build();
            var wireQuery = SystemAPI.QueryBuilder().WithAll<Wire>().Build();

            var allNodes = nodeQuery.ToEntityArray(state.WorldUpdateAllocator);
            var allWires = wireQuery.ToEntityArray(state.WorldUpdateAllocator);

            var job = new SolveCircuitJob
            {
                AllNodes = allNodes,
                AllWires = allWires,
                NodeLookup = nodeLookup,
                WireLookup = wireLookup,
                BufferLookup = bufferLookup
            };

            state.Dependency = job.Schedule(state.Dependency);

            if (control.StepNextFrame)
            {
                control.StepNextFrame = false;
                SystemAPI.SetSingleton(control);
            }
        }
    }

    [BurstCompile]
    public struct SolveCircuitJob : IJob
    {
        [ReadOnly] public NativeArray<Entity> AllNodes;
        [ReadOnly] public NativeArray<Entity> AllWires;
        public ComponentLookup<ElectricalNode> NodeLookup;
        public ComponentLookup<Wire> WireLookup;
        [ReadOnly] public BufferLookup<ConnectedWireBuffer> BufferLookup;

        public void Execute()
        {
            // --- 0. СБРОС ТОКОВ ВО ВСЕХ ПРОВОДАХ ---
            for (int i = 0; i < AllWires.Length; i++)
            {
                var wire = WireLookup[AllWires[i]];
                wire.CurrentFlow = 0f;
                WireLookup[AllWires[i]] = wire;
            }

            // --- 1. ГЛОБАЛЬНЫЙ СБРОС УЗЛОВ ---
            for (int i = 0; i < AllNodes.Length; i++)
            {
                var entity = AllNodes[i];
                var node = NodeLookup[entity];

                node.CurrentVoltage = node.IsPowerSource ? node.SourceVoltage : 0f;
                node.CurrentHasGround = node.IsGrounded;
                node.PathResistance = node.Resistance; // На старте путь равен собственному сопротивлению
                node.ParentWire = Entity.Null;         // Очищаем "хлебные крошки"

                NodeLookup[entity] = node;
            }

            // --- 2. ПРОЗВОНКА ОТ ПЛЮСА И РАСЧЕТ ТОКОВ ---
            NativeQueue<Entity> queue = new NativeQueue<Entity>(Allocator.Temp);

            for (int i = 0; i < AllNodes.Length; i++)
            {
                if (NodeLookup[AllNodes[i]].IsPowerSource) queue.Enqueue(AllNodes[i]);
            }

            while (queue.TryDequeue(out Entity currentEntity))
            {
                var currentNode = NodeLookup[currentEntity];
                if (!BufferLookup.HasBuffer(currentEntity)) continue;

                var buffers = BufferLookup[currentEntity];
                for (int i = 0; i < buffers.Length; i++)
                {
                    Entity wireEntity = buffers[i].WireEntity;
                    if (!WireLookup.HasComponent(wireEntity)) continue;

                    Wire wire = WireLookup[wireEntity];
                    if (wire.IsBroken || !wire.IsConducting) continue;

                    Entity neighbor = (wire.NodeA == currentEntity) ? wire.NodeB : wire.NodeA;
                    if (!NodeLookup.HasComponent(neighbor)) continue;

                    var neighborNode = NodeLookup[neighbor];

                    // ЕСЛИ МЫ ДОШЛИ ДО ЗЕМЛИ - ЦЕПЬ ЗАМКНУЛАСЬ!
                    if (neighborNode.IsGrounded)
                    {
                        // Считаем полное сопротивление пути: всё, что накопили + этот провод
                        float totalResistance = currentNode.PathResistance + wire.Resistance;

                        // Защита от деления на 0. Если сопротивления нет вообще - ставим минимальное для КЗ
                        if (totalResistance < 0.01f) totalResistance = 0.01f;

                        // ЗАКОН ОМА: Ток = Напряжение / Сопротивление
                        float currentAmps = currentNode.CurrentVoltage / totalResistance;

                        // === ОБРАТНАЯ ТРАССИРОВКА: пускаем ток по найденному пути ===
                        Entity traceWireEntity = wireEntity;
                        Entity traceNodeEntity = currentEntity;

                        // Записываем ток в финальный провод, который коснулся земли
                        var finalWire = WireLookup[traceWireEntity];
                        finalWire.CurrentFlow += currentAmps;
                        if (finalWire.CurrentFlow > finalWire.MaxCurrent) finalWire.IsBroken = true; // КЗ!
                        WireLookup[traceWireEntity] = finalWire;

                        // Бежим по "хлебным крошкам" обратно к источнику питания
                        while (true)
                        {
                            var tNode = NodeLookup[traceNodeEntity];
                            if (tNode.ParentWire == Entity.Null) break; // Дошли до источника

                            traceWireEntity = tNode.ParentWire;
                            var tWire = WireLookup[traceWireEntity];

                            tWire.CurrentFlow += currentAmps;
                            if (tWire.CurrentFlow > tWire.MaxCurrent) tWire.IsBroken = true; // КЗ!

                            WireLookup[traceWireEntity] = tWire;

                            // Шагаем на узел назад
                            traceNodeEntity = (tWire.NodeA == traceNodeEntity) ? tWire.NodeB : tWire.NodeA;
                        }

                        continue; // Землю напряжением не заражаем
                    }

                    // Если это не земля, передаем напряжение дальше
                    if (neighborNode.CurrentVoltage < currentNode.CurrentVoltage)
                    {
                        neighborNode.CurrentVoltage = currentNode.CurrentVoltage;
                        neighborNode.ParentWire = wireEntity; // ОСТАВЛЯЕМ ХЛЕБНУЮ КРОШКУ
                        neighborNode.PathResistance = currentNode.PathResistance + wire.Resistance + neighborNode.Resistance;
                        NodeLookup[neighbor] = neighborNode;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // --- 3. ПРОЗВОНКА ЗЕМЛИ (Остается без изменений для логики реле) ---
            for (int i = 0; i < AllNodes.Length; i++)
            {
                if (NodeLookup[AllNodes[i]].IsGrounded) queue.Enqueue(AllNodes[i]);
            }

            while (queue.TryDequeue(out Entity currentEntity))
            {
                if (!BufferLookup.HasBuffer(currentEntity)) continue;
                var buffers = BufferLookup[currentEntity];
                for (int i = 0; i < buffers.Length; i++)
                {
                    Entity wireEntity = buffers[i].WireEntity;
                    if (!WireLookup.HasComponent(wireEntity)) continue;
                    Wire wire = WireLookup[wireEntity];
                    if (wire.IsBroken || !wire.IsConducting) continue;

                    Entity neighbor = (wire.NodeA == currentEntity) ? wire.NodeB : wire.NodeA;
                    if (!NodeLookup.HasComponent(neighbor)) continue;

                    var neighborNode = NodeLookup[neighbor];
                    if (!neighborNode.CurrentHasGround)
                    {
                        neighborNode.CurrentHasGround = true;
                        NodeLookup[neighbor] = neighborNode;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            queue.Dispose();
        }
    }
}