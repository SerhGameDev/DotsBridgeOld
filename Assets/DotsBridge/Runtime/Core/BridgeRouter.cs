using Unity.Entities;

namespace DotsBridge
{
    /// <summary>
    /// Единая точка входа для всех запросов. Сделана как partial struct, 
    /// чтобы ты мог легко добавлять сюда свои методы расширения из любых файлов!
    /// </summary>
    public readonly partial struct BridgeRouter
    {
        public readonly BridgeState State;

        public BridgeRouter(BridgeState state)
        {
            State = state;
        }

        // =========================================================
        // ВСЕ МЕТОДЫ ПИШУТСЯ ТОЛЬКО ОДИН РАЗ ЗДЕСЬ
        // =========================================================

        public SpawnerBuilder BeginSpawn(string prefabName)
        {
            return new SpawnerBuilder(State, EntityBridge.GetPrefabInternal(State, prefabName));
        }

        public EntityBatch GetById(string id)
        {
            return EntityBridge.GetByIdInternal(State, id);
        }

        public EntityBatch GetByTags(params string[] tags)
        {
            // Твоя логика поиска по тегам...
            return new EntityBatch(default, State);
        }
    }
}