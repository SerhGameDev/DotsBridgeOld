using System;

namespace DotsBridge
{
    public partial class DotsCommand
    {
        public readonly string Name;

        // Обрати внимание: мы удалили _explicitState и методы OnServer/OnClient!

        // Резолвер принимает BridgeState в момент вызова Execute
        private Func<BridgeState, EntityBatch> _targetResolver;
        private bool _requiresDispose;
        private Action<EntityBatch> _actions;

        public DotsCommand(string name)
        {
            Name = name;
        }

        // =========================================================
        // ВЫБОР ЦЕЛИ
        // =========================================================

        public DotsCommand GetById(string id)
        {
            _targetResolver = (state) => EntityBridge.GetByIdInternal(state, id);
            _requiresDispose = false;
            return this;
        }

        public DotsCommand Do(Action<EntityBatch> customAction)
        {
            _actions += customAction;
            return this;
        }

        // =========================================================
        // ТЕРМИНАЛЬНЫЕ МЕТОДЫ (EXECUTE)
        // =========================================================

        /// <summary> Выполняет команду на Сервере </summary>
        public void ServerExecute() => ExecuteInternal(EntityBridge.ServerState, "Server");

        /// <summary> Выполняет команду на Клиенте </summary>
        public void ClientExecute() => ExecuteInternal(EntityBridge.ClientState, "Client");

        /// <summary> Автоматически определяет мир (для синглплеера) </summary>
        public void Execute() => ExecuteInternal(EntityBridge.GetActiveState(), "Auto");

        // Внутренняя логика выполнения (скрыта от пользователя)
        private void ExecuteInternal(BridgeState activeState, string contextName)
        {
            if (_targetResolver == null)
            {
                UnityEngine.Debug.LogError($"[DotsCommand] Ошибка: Не указана цель для '{Name}'.");
                return;
            }

            if (activeState == null)
            {
                UnityEngine.Debug.LogError($"[DotsCommand] Ошибка: Мир '{contextName}' не найден для выполнения команды '{Name}'!");
                return;
            }

            // Ищем батч в переданном мире
            EntityBatch batch = _targetResolver.Invoke(activeState);

            // Выполняем действия
            _actions?.Invoke(batch);

            if (_requiresDispose)
                batch.Dispose();
        }
    }
}