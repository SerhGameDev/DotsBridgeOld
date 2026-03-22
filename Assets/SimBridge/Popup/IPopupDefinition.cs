using DotsBridge.UI.Positioning;

namespace DotsBridge.UI
{
    /// <summary>
    /// Контракт для описания логики конкретного всплывающего окна.
    /// </summary>
    public interface IPopupDefinition
    {
        /// <summary>
        /// Условие: подходит ли данная сущность для этого окна?
        /// </summary>
        bool CanHandle(SingleEntity entity);

        /// <summary>
        /// Сборка данных для отображения.
        /// </summary>
        void BuildContent(SingleEntity entity, PopupBuilder builder);

        /// <summary>
        /// Выбор стратегии позиционирования (за курсором, по центру, в панели).
        /// </summary>
        IPopupPositionStrategy GetStrategy();

        // TODO в будущем:
        // void BindCommands(VisualElement popupRoot, SingleEntity entity);
    }
}