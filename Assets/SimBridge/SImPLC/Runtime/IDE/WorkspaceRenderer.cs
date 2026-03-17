using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public class WorkspaceRenderer
    {
        private readonly VisualElement _container;
        private WorkArea _currentWorkArea;
        
        private readonly WorkspaceGrid _grid;
        private readonly VisualElement _nodeContainer;

        public WorkspaceRenderer(VisualElement container)
        {
            _container = container;
            _container.style.flexGrow = 1;
            _container.style.overflow = Overflow.Hidden; // Обрезаем элементы, выходящие за край
            _container.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f);

            // 1. Добавляем фон-сетку
            _grid = new WorkspaceGrid();
            _container.Add(_grid);

            // 2. Добавляем контейнер для нод поверх сетки
            _nodeContainer = new VisualElement();
            _nodeContainer.style.flexGrow = 1;
            _nodeContainer.style.position = Position.Absolute;
            _nodeContainer.style.left = 0; _nodeContainer.style.top = 0; 
            _nodeContainer.style.right = 0; _nodeContainer.style.bottom = 0;
            _container.Add(_nodeContainer);
        }

        public void Render(WorkArea workArea)
        {
            if (_currentWorkArea == workArea) return;
            _currentWorkArea = workArea;
            
            // Очищаем старые ноды при переключении файла
            _nodeContainer.Clear();

            if (workArea == null) return;

            // Рисуем новые ноды
            foreach (var nodeData in workArea.Nodes)
            {
                DrawNode(nodeData);
            }
        }

        private void DrawNode(Node nodeData)
        {
            // Создаем визуальную карточку ноды
            var nodeVisual = new VisualElement();
            nodeVisual.style.position = Position.Absolute;
            nodeVisual.style.left = nodeData.Position.x;
            nodeVisual.style.top = nodeData.Position.y;
            
            // Базовый дизайн ноды прямо в коде (позже можно вынести в USS или UXML)
            nodeVisual.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);
            nodeVisual.style.borderTopColor = nodeVisual.style.borderBottomColor = 
            nodeVisual.style.borderLeftColor = nodeVisual.style.borderRightColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            nodeVisual.style.borderTopWidth = nodeVisual.style.borderBottomWidth = 
            nodeVisual.style.borderLeftWidth = nodeVisual.style.borderRightWidth = 1;
            nodeVisual.style.borderTopLeftRadius = nodeVisual.style.borderTopRightRadius = 
            nodeVisual.style.borderBottomLeftRadius = nodeVisual.style.borderBottomRightRadius = 6;
            nodeVisual.style.paddingTop = nodeVisual.style.paddingBottom = 8;
            nodeVisual.style.paddingLeft = nodeVisual.style.paddingRight = 12;
            nodeVisual.style.minWidth = 120;
            nodeVisual.style.minHeight = 40;

            // Заголовок ноды
            var title = new Label(nodeData.Name);
            title.style.color = Color.white;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            nodeVisual.Add(title);
            _nodeContainer.Add(nodeVisual);
        }
    }
}