using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public abstract class BaseContextMenu : IDisposable
    {
        // Главный контейнер окна меню
        private const string MenuContainerName = "context-menu-container";
        // Label для установки имени действия
        private const string ItemLabelName = "menu-item-label";

        protected readonly VisualElement Root;
        private readonly VisualTreeAsset _itemTemplate;
        private readonly VisualElement _menuContainer;

        protected BaseContextMenu(VisualElement root, VisualTreeAsset menuTemplate, VisualTreeAsset itemTemplate)
        {
            Root = root;
            _itemTemplate = itemTemplate;

            var menuRoot = menuTemplate.Instantiate();
            _menuContainer = menuRoot.Q<VisualElement>(MenuContainerName);
            
            _menuContainer.style.display = DisplayStyle.None;
            Root.Add(_menuContainer);

            // Закрываем меню при клике мимо него
            Root.RegisterCallback<PointerDownEvent>(OnPointerDownOutside, TrickleDown.TrickleDown);
        }

        public void Show(Vector2 position)
        {
            _menuContainer.Clear();

            var actions = GetActions();
            if (actions == null || actions.Count == 0) return;

            foreach (var action in actions)
            {
                var itemElement = _itemTemplate.Instantiate();
                var label = itemElement.Q<Label>(ItemLabelName);
                if (label != null) label.text = action.Name;

                // Визуальный отклик при наведении (имитация hover из USS кодом для изоляции компонента)
                itemElement.RegisterCallback<PointerEnterEvent>(e => itemElement.style.backgroundColor = new Color(0.2f, 0.4f, 0.8f, 1f));
                itemElement.RegisterCallback<PointerLeaveEvent>(e => itemElement.style.backgroundColor = StyleKeyword.Null);

                // Выполнение команды при левом клике
                itemElement.RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (evt.button == 0) 
                    {
                        action.Command?.Invoke();
                        Hide();
                        evt.StopPropagation();
                    }
                });

                _menuContainer.Add(itemElement);
            }

            _menuContainer.style.left = position.x;
            _menuContainer.style.top = position.y;
            _menuContainer.style.display = DisplayStyle.Flex;
            _menuContainer.BringToFront(); // Меню всегда поверх остальных элементов
        }

        public void Hide()
        {
            _menuContainer.style.display = DisplayStyle.None;
        }

        private void OnPointerDownOutside(PointerDownEvent evt)
        {
            if (_menuContainer.style.display == DisplayStyle.Flex)
            {
                bool clickedInsideMenu = _menuContainer.Contains(evt.target as VisualElement) || evt.target == _menuContainer;
                if (!clickedInsideMenu)
                {
                    Hide();
                }
            }
        }

        // Наследники должны вернуть список действий, доступных в текущем контексте
        protected abstract List<ContextMenuAction> GetActions();

        public virtual void Dispose()
        {
            Root.UnregisterCallback<PointerDownEvent>(OnPointerDownOutside, TrickleDown.TrickleDown);
            if (_menuContainer != null && _menuContainer.parent != null)
            {
                _menuContainer.parent.Remove(_menuContainer);
            }
        }
    }
}