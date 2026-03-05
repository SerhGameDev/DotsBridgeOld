using System;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    /// <summary>
    /// Контейнер для отложенного выполнения цепочки команд над DOTS-сущностями.
    /// </summary>
    public partial class DotsCommand
    {
        public readonly string Name;
        private Func<EntityBatch> _targetResolver;
        private bool _requiresDispose;
        private Action<EntityBatch> _actions;

        public DotsCommand(string name) => Name = name;

        public DotsCommand Do(Action<EntityBatch> customAction)
        {
            _actions += customAction;
            return this;
        }

        public void Execute()
        {
            if (_targetResolver == null) { /* Error handling */ return; }
            EntityBatch batch = _targetResolver.Invoke();
            _actions?.Invoke(batch);
            if (_requiresDispose) batch.Dispose();
        }
    }
}