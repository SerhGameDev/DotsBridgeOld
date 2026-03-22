using System;
using DotsBridge.UI.Positioning;

namespace DotsBridge.UI
{
    /// <summary>
    /// Адаптер для регистрации окон через быстрые лямбда-выражения.
    /// </summary>
    internal class AnonymousPopupDefinition : IPopupDefinition
    {
        private readonly Func<SingleEntity, bool> _condition;
        private Action<SingleEntity, PopupBuilder> _buildAction;
        private IPopupPositionStrategy _strategy = new MouseCursorStrategy();

        public AnonymousPopupDefinition(Func<SingleEntity, bool> condition) => _condition = condition;

        public bool CanHandle(SingleEntity entity) => _condition(entity);

        public void BuildContent(SingleEntity entity, PopupBuilder builder) => _buildAction?.Invoke(entity, builder);

        public IPopupPositionStrategy GetStrategy() => _strategy;

        // Методы для настройки анонимного определения
        public void SetBuild(Action<SingleEntity, PopupBuilder> build) => _buildAction = build;
        public void SetStrategy(IPopupPositionStrategy strategy) => _strategy = strategy;
    }
}