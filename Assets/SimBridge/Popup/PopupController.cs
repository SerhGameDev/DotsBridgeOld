using UnityEngine.UIElements;

namespace DotsBridge.UI
{
    public class PopupController
    {
        private readonly PopupManager _manager;
        private readonly IPopupDefinition _definition;
        private VisualElement _instance;

        public PopupController(PopupManager manager, IPopupDefinition definition)
        {
            _manager = manager;
            _definition = definition;
        }

        public void TryOpen(SingleEntity entity, VisualElement root)
        {
            if (!_definition.CanHandle(entity)) return;

            var builder = new PopupBuilder();
            _definition.BuildContent(entity, builder);

            _instance = _manager.InstantiateBasePopup();
            var container = _instance.Q<VisualElement>("PopupContent") ?? _instance;

            foreach (var row in builder.Rows)
            {
                var rowElement = _manager.CreateRowElement(row);
                if (rowElement != null) container.Add(rowElement);
            }

            var strategy = _definition.GetStrategy();
            strategy.OnCreated(_instance, root);
            strategy.ApplyPosition(_instance);
        }

        public void UpdatePosition()
        {
            if (_instance != null)
            {
                _definition.GetStrategy().ApplyPosition(_instance);
            }
        }

        public void Close(VisualElement root)
        {
            if (_instance != null)
            {
                _definition.GetStrategy().OnDestroyed(_instance, root);
                if (_instance.parent != null) _instance.parent.Remove(_instance);
                _instance = null;
            }
        }
    }
}