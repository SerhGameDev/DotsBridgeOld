using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimPLS
{
    public class ContextMenu : VisualElement
    {
        public Action<Type, Vector2> OnNodeSelected; // Событие: какую ноду выбрали и где
        public Action OnClose;                       // Событие: закрыть меню

        private Vector2 spawnPosition;               // Координаты, где кликнули мышкой
        private TextField searchField;
        private ScrollView scrollView;

        // Храним ссылки на созданные кнопки и заголовки для фильтрации поиска
        private Dictionary<Button, string> itemButtons = new Dictionary<Button, string>();
        private Dictionary<Label, List<Button>> categoryGroups = new Dictionary<Label, List<Button>>();

        public ContextMenu(List<NodeCreationData> availableNodes)
        {
            // Базовый стиль окошка (можно потом вынести в USS класс)
            this.style.position = Position.Absolute;
            this.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.95f); // Темный фон
            this.style.borderBottomColor = this.style.borderTopColor = this.style.borderLeftColor = this.style.borderRightColor = Color.black;
            this.style.borderBottomWidth = this.style.borderTopWidth = this.style.borderLeftWidth = this.style.borderRightWidth = 1;
            this.style.borderBottomLeftRadius = this.style.borderBottomRightRadius = this.style.borderTopLeftRadius = this.style.borderTopRightRadius = 5;
            this.style.width = 200;
            this.style.maxHeight = 300;
            this.style.paddingBottom = this.style.paddingTop = this.style.paddingLeft = this.style.paddingRight = 5;

            // 1. Поле поиска
            searchField = new TextField();
            searchField.RegisterValueChangedCallback(OnSearchTextChanged);
            this.Add(searchField);

            // 2. Скролл для списка
            scrollView = new ScrollView();
            this.Add(scrollView);

            // 3. Группируем ноды по категориям и создаем UI
            var groupedNodes = new Dictionary<string, List<NodeCreationData>>();
            foreach (var node in availableNodes)
            {
                if (!groupedNodes.ContainsKey(node.Category))
                    groupedNodes[node.Category] = new List<NodeCreationData>();
                groupedNodes[node.Category].Add(node);
            }

            foreach (var category in groupedNodes)
            {
                // Заголовок категории (Math, Logic и т.д.)
                Label categoryLabel = new Label(category.Key);
                categoryLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                categoryLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                categoryLabel.style.marginTop = 5;
                categoryLabel.style.marginBottom = 2;
                scrollView.Add(categoryLabel);

                List<Button> buttonsInCategory = new List<Button>();

                // Кнопки нод внутри категории
                foreach (var nodeData in category.Value)
                {
                    Button btn = new Button(() => SelectNode(nodeData.NodeType));
                    btn.text = nodeData.MenuName;
                    btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                    btn.style.backgroundColor = Color.clear; // Прозрачный фон по умолчанию
                    btn.style.borderBottomWidth = btn.style.borderTopWidth = btn.style.borderLeftWidth = btn.style.borderRightWidth = 0;
                    btn.style.color = Color.white;

                    // Эффект наведения
                    btn.RegisterCallback<MouseEnterEvent>(e => btn.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f));
                    btn.RegisterCallback<MouseLeaveEvent>(e => btn.style.backgroundColor = Color.clear);

                    scrollView.Add(btn);
                    itemButtons.Add(btn, nodeData.MenuName.ToLower()); 
                    buttonsInCategory.Add(btn);
                }

                categoryGroups.Add(categoryLabel, buttonsInCategory);
            }

            this.RegisterCallback<GeometryChangedEvent>(e => searchField.Focus());
        }

        public void Show(Vector2 screenPosition, Vector2 localWorkspacePosition)
        {
            spawnPosition = localWorkspacePosition;
            this.style.left = screenPosition.x;
            this.style.top = screenPosition.y;
            this.style.display = DisplayStyle.Flex;
            
            // Сбрасываем поиск
            searchField.value = "";
        }

        public void Hide()
        {
            this.style.display = DisplayStyle.None;
            OnClose?.Invoke();
        }

        private void SelectNode(Type nodeType)
        {
            OnNodeSelected?.Invoke(nodeType, spawnPosition);
            Hide();
        }

        // --- ЛОГИКА ПОИСКА И ФИЛЬТРАЦИИ ---
        private void OnSearchTextChanged(ChangeEvent<string> evt)
        {
            string query = evt.newValue.ToLower();

            foreach (var categoryKvp in categoryGroups)
            {
                Label categoryLabel = categoryKvp.Key;
                List<Button> buttons = categoryKvp.Value;
                bool hasVisibleButtons = false;

                foreach (var btn in buttons)
                {
                    string nodeName = itemButtons[btn];
                    // Если строка поиска пустая или имя содержит запрос
                    if (string.IsNullOrEmpty(query) || nodeName.Contains(query))
                    {
                        btn.style.display = DisplayStyle.Flex; // Показываем
                        hasVisibleButtons = true;
                    }
                    else
                    {
                        btn.style.display = DisplayStyle.None; // Скрываем
                    }
                }

                // Если в категории не осталось видимых кнопок, скрываем и сам заголовок категории
                categoryLabel.style.display = hasVisibleButtons ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}