using System;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge
{
    public partial class DotsCommand
    {

        /// <summary>
        /// Позволяет внедрить кастомную логику получения (или создания) сущностей.
        /// Используется, например, в SpawnerBuilder.
        /// </summary>
        public DotsCommand SetTargetResolver(Func<EntityBatch> resolver, bool requiresDispose)
        {
            _targetResolver = resolver;
            _requiresDispose = requiresDispose;
            return this;
        }

        // --- НАКОПЛЕНИЕ ДЕЙСТВИЙ (ACTIONS) ---

        public DotsCommand AddComponent<T>() where T : unmanaged, IComponentData
        {
            _actions += batch => batch.AddComponent<T>();
            return this;
        }

        public DotsCommand SetData<T>(T data) where T : unmanaged, IComponentData
        {
            _actions += batch => batch.SetData(data);
            return this;
        }

        public DotsCommand LogCount(string prefix = "Command executed on entities:")
        {
            _actions += batch => Debug.Log($"[{Name}] {prefix} {batch.Entities.Length}");
            return this;
        }
    }
}