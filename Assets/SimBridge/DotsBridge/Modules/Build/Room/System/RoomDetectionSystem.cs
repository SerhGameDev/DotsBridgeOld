using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace DotsBridge.Build
{
    [UpdateAfter(typeof(WallConnectionSystem))]
    [BurstCompile]
    public partial struct RoomDetectionSystem : ISystem
    {
        private EntityQuery _topologyChangedQuery;
        private EntityQuery _allWallsQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _topologyChangedQuery = state.GetEntityQuery(typeof(TopologyChangedTag));
            _allWallsQuery = state.GetEntityQuery(typeof(WallComponent), typeof(WallTag));
            state.RequireForUpdate(_topologyChangedQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var wallEntities = _allWallsQuery.ToEntityArray(Allocator.TempJob);
            var wallComponents = _allWallsQuery.ToComponentDataArray<WallComponent>(Allocator.TempJob);

            var job = new FindRoomsJob
            {
                Walls = wallEntities,
                WallData = wallComponents,
                ECB = ecb
            };

            state.Dependency = job.Schedule(state.Dependency);
            var triggers = _topologyChangedQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < triggers.Length; i++)
            {
                ecb.DestroyEntity(triggers[i]);
            }
            triggers.Dispose();
        }

        // Внутренняя структура полуребра для алгоритма
        private struct HalfEdge : System.IComparable<HalfEdge>
        {
            public int Id;
            public int OriginVertex;
            public int TargetVertex;
            public Entity WallEntity;
            public float Angle; // Угол от Origin к Target
            public int TwinId;
            public int NextId;
            public bool Visited;

            // Сортировка по углу (по часовой стрелке)
            public int CompareTo(HalfEdge other)
            {
                return other.Angle.CompareTo(Angle); 
            }
        }

        [BurstCompile]
        private struct FindRoomsJob : IJob
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> Walls;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<WallComponent> WallData;
            public EntityCommandBuffer ECB;

            public void Execute()
            {
                if (Walls.Length == 0) return;

                // 1. Собираем уникальные вершины (квантуем координаты в int2 для надежности хэша)
                var vertexMap = new NativeHashMap<int2, int>(Walls.Length * 2, Allocator.Temp);
                var vertexPositions = new NativeList<float3>(Allocator.Temp);
                
                // 2. Создаем полуребра
                var edges = new NativeList<HalfEdge>(Allocator.Temp);
                var vertexOutEdges = new NativeParallelMultiHashMap<int, int>(Walls.Length * 2, Allocator.Temp);

                for (int i = 0; i < Walls.Length; i++)
                {
                    var wall = WallData[i];
                    int2 startP = new int2((int)math.round(wall.Start.x * 100f), (int)math.round(wall.Start.z * 100f));
                    int2 endP = new int2((int)math.round(wall.End.x * 100f), (int)math.round(wall.End.z * 100f));

                    int startId = GetOrAddVertex(startP, wall.Start, ref vertexMap, ref vertexPositions);
                    int endId = GetOrAddVertex(endP, wall.End, ref vertexMap, ref vertexPositions);

                    // Углы в радианах
                    float angle1 = math.atan2(wall.End.z - wall.Start.z, wall.End.x - wall.Start.x);
                    float angle2 = math.atan2(wall.Start.z - wall.End.z, wall.Start.x - wall.End.x);

                    int edgeId1 = edges.Length;
                    int edgeId2 = edges.Length + 1;

                    edges.Add(new HalfEdge { Id = edgeId1, OriginVertex = startId, TargetVertex = endId, WallEntity = Walls[i], Angle = angle1, TwinId = edgeId2, NextId = -1 });
                    edges.Add(new HalfEdge { Id = edgeId2, OriginVertex = endId, TargetVertex = startId, WallEntity = Walls[i], Angle = angle2, TwinId = edgeId1, NextId = -1 });

                    vertexOutEdges.Add(startId, edgeId1);
                    vertexOutEdges.Add(endId, edgeId2);
                }

                // 3. Сортируем исходящие ребра и линкуем Next
                LinkHalfEdges(ref edges, ref vertexOutEdges, vertexPositions.Length);

                // 4. Обход граней (Faces)
                FindFacesAndCreateRooms(ref edges, ref vertexPositions);

                vertexMap.Dispose();
                vertexPositions.Dispose();
                edges.Dispose();
                vertexOutEdges.Dispose();
            }

            private int GetOrAddVertex(int2 hash, float3 pos, ref NativeHashMap<int2, int> map, ref NativeList<float3> positions)
            {
                if (map.TryGetValue(hash, out int id)) return id;
                id = positions.Length;
                positions.Add(pos);
                map.Add(hash, id);
                return id;
            }

            private void LinkHalfEdges(ref NativeList<HalfEdge> edges, ref NativeParallelMultiHashMap<int, int> vertexOutEdges, int vertexCount)
            {
                var sortedEdges = new NativeList<HalfEdge>(Allocator.Temp);

                for (int v = 0; v < vertexCount; v++)
                {
                    sortedEdges.Clear();
                    
                    if (vertexOutEdges.TryGetFirstValue(v, out int edgeId, out var iterator))
                    {
                        do { sortedEdges.Add(edges[edgeId]); } 
                        while (vertexOutEdges.TryGetNextValue(out edgeId, ref iterator));
                    }

                    if (sortedEdges.Length == 0) continue;

                    // Сортируем ребра по часовой стрелке
                    sortedEdges.Sort();

                    // Линкуем Next: Next для ребра, ВХОДЯЩЕГО в вершину v,
                    // это исходящее из v ребро, которое идет следующим в отсортированном списке после его Twin.
                    for (int i = 0; i < sortedEdges.Length; i++)
                    {
                        int currentTwinId = sortedEdges[i].TwinId;
                        int nextOutgoingIndex = (i + 1) % sortedEdges.Length;
                        int nextOutgoingId = sortedEdges[nextOutgoingIndex].Id;

                        var twinEdge = edges[currentTwinId];
                        twinEdge.NextId = nextOutgoingId;
                        edges[currentTwinId] = twinEdge;
                    }
                }
                sortedEdges.Dispose();
            }

            private void FindFacesAndCreateRooms(ref NativeList<HalfEdge> edges, ref NativeList<float3> positions)
            {
                var faceVertices = new NativeList<float3>(Allocator.Temp);
                var faceWalls = new NativeList<Entity>(Allocator.Temp);

                for (int i = 0; i < edges.Length; i++)
                {
                    var startEdge = edges[i];
                    if (startEdge.Visited) continue;

                    faceVertices.Clear();
                    faceWalls.Clear();

                    int currentId = i;
                    bool isClosed = false;

                    // Идем по связям Next
                    do
                    {
                        var edge = edges[currentId];
                        edge.Visited = true;
                        edges[currentId] = edge;

                        faceVertices.Add(positions[edge.OriginVertex]);
                        faceWalls.Add(edge.WallEntity);

                        currentId = edge.NextId;

                        if (currentId == i) isClosed = true;

                    } while (currentId != i && currentId != -1 && !edges[currentId].Visited);

                    // Если цикл замкнулся
                    if (isClosed && faceVertices.Length >= 3)
                    {
                        float area = CalculatePolygonArea(faceVertices);
                        
                        // Положительная площадь (при нашей сортировке) означает внутреннюю комнату.
                        // Отрицательная — это "оболочка" (периметр всего здания). Мы её игнорируем.
                        if (area > 0.01f)
                        {
                            float perimeter = CalculatePolygonPerimeter(faceVertices);

                            Entity roomEntity = ECB.CreateEntity();
                            ECB.AddComponent<RoomTag>(roomEntity);
                            ECB.AddComponent(roomEntity, new RoomData { 
                                Area = area, 
                                Perimeter = perimeter 
                            });
    
                            var buffer = ECB.AddBuffer<RoomWallElement>(roomEntity);
                            for (int w = 0; w < faceWalls.Length; w++)
                            {
                                buffer.Add(new RoomWallElement { WallEntity = faceWalls[w] });
                            }
                        }
                    }
                }
                faceVertices.Dispose();
                faceWalls.Dispose();
            }

            private float CalculatePolygonArea(NativeList<float3> vertices)
            {
                float area = 0f;
                for (int i = 0; i < vertices.Length; i++)
                {
                    int j = (i + 1) % vertices.Length;
                    area += (vertices[i].x * vertices[j].z) - (vertices[j].x * vertices[i].z);
                }
                return area * 0.5f; 
            }
            private float CalculatePolygonPerimeter(NativeList<float3> vertices)
            {
                float perimeter = 0f;
                int j = vertices.Length - 1;
                for (int i = 0; i < vertices.Length; i++)
                {
                    perimeter += math.distance(vertices[j], vertices[i]);
                    j = i;
                }
                return perimeter;
            }
        }
    }
}