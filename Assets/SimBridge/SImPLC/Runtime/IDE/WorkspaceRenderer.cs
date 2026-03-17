using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public class WorkspaceRenderer
    {
        private readonly VisualElement _container;
        private readonly VisualTreeAsset _nodeTemplate;
        
        private WorkArea _currentWorkArea;
        
        private readonly WorkspaceGrid _grid;
        private readonly VisualElement _nodeContainer;

        public WorkspaceRenderer(VisualElement container, VisualTreeAsset nodeTemplate)
        {
            _container = container;
            _nodeTemplate = nodeTemplate;
            
            _container.style.flexGrow = 1;
            _container.style.overflow = Overflow.Hidden;
            _container.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f);

            _grid = new WorkspaceGrid();
            _container.Add(_grid);

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

            // Отписываемся от старой рабочей области, если она была
            if (_currentWorkArea != null)
            {
                _currentWorkArea.OnNodeAdded -= DrawNode;
                _currentWorkArea.OnNodeRemoved -= EraseNode;
            }

            _currentWorkArea = workArea;
            _nodeContainer.Clear();

            if (_currentWorkArea == null) return;

            // Подписываемся на новую рабочую область
            _currentWorkArea.OnNodeAdded += DrawNode;
            _currentWorkArea.OnNodeRemoved += EraseNode;

            // Отрисовываем существующие ноды
            foreach (var nodeData in _currentWorkArea.Nodes)
            {
                DrawNode(nodeData);
            }
        }

        private void DrawNode(Node nodeData)
        {
            // Создаем из UXML
            var nodeVisual = _nodeTemplate.Instantiate();
            
            // Настраиваем позиционирование корня UXML
            nodeVisual.style.position = Position.Absolute;
            nodeVisual.style.left = nodeData.Position.x;
            nodeVisual.style.top = nodeData.Position.y;
            
            // Записываем ID в userData, чтобы потом легко найти элемент для удаления
            nodeVisual.userData = nodeData.Id;

            // Настраиваем данные
            var title = nodeVisual.Q<Label>("node-title");
            if (title != null) title.text = nodeData.Name;

            _nodeContainer.Add(nodeVisual);
        }

        private void EraseNode(string nodeId)
        {
            // Ищем визуальный элемент по ID и удаляем его со сцены
            var nodeVisual = _nodeContainer.Children().FirstOrDefault(x => x.userData as string == nodeId);
            if (nodeVisual != null)
            {
                _nodeContainer.Remove(nodeVisual);
            }
        }
    }
}