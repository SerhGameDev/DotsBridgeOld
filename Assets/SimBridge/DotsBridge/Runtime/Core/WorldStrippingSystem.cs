using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
public partial struct WorldEntityFilterSystem : ISystem
{
    private EntityQuery _serverOnlyQuery;
    private EntityQuery _clientOnlyQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Готовим запросы заранее для максимальной скорости
        _serverOnlyQuery = state.GetEntityQuery(ComponentType.ReadOnly<ServerOnlyTag>());
        _clientOnlyQuery = state.GetEntityQuery(ComponentType.ReadOnly<ClientOnlyTag>());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Логика для КЛИЕНТСКОГО мира
        if (state.World.IsClient())
        {
            // Если на клиенте нашли сущность "Только для Сервера" — удаляем её целиком
            if (!_serverOnlyQuery.IsEmptyIgnoreFilter)
            {
                state.EntityManager.DestroyEntity(_serverOnlyQuery);
            }
        }

        // Логика для СЕРВЕРНОГО мира
        if (state.World.IsServer())
        {
            // Если на сервере нашли сущность "Только для Клиента" — удаляем
            if (!_clientOnlyQuery.IsEmptyIgnoreFilter)
            {
                state.EntityManager.DestroyEntity(_clientOnlyQuery);
            }
        }
    }
}