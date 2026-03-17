using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public class HierarchyDragAndDropController
    {
        private readonly ScrollView _scrollView;
        private readonly Dictionary<string, HierarchyViewElement> _elements;
        private readonly List<VisualElement> _pickList = new List<VisualElement>();

        private VisualElement _dragGhost;
        private HierarchyViewElement _currentDragTarget;

        public event Action<string, string> OnItemMoveRequested;

        public HierarchyDragAndDropController(ScrollView scrollView, Dictionary<string, HierarchyViewElement> elements)
        {
            _scrollView = scrollView;
            _elements = elements;
        }

        public void HandleDragStart(HierarchyDragManipulator manipulator, Vector2 position)
        {
            var original = manipulator.Element.Root;
            original.style.opacity = 0.3f;

            _dragGhost = new Label(manipulator.Element.Data.Name);
            _dragGhost.style.position = Position.Absolute;
            _dragGhost.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            _dragGhost.style.borderBottomColor = _dragGhost.style.borderTopColor = Color.cyan;
            _dragGhost.style.borderLeftWidth = _dragGhost.style.borderRightWidth = 2;
            _dragGhost.style.paddingLeft = _dragGhost.style.paddingRight = 5;
            _dragGhost.pickingMode = PickingMode.Ignore;

            _scrollView.panel.visualTree.Add(_dragGhost);
            UpdateGhostPosition(position);
        }
        
        public void Attach(HierarchyViewElement element)
        {
            // Создаем манипулятор и передаем ему ссылки на методы обработки этого контроллера
            var dragManipulator = new HierarchyDragManipulator(
                element, 
                HandleDragStart, 
                HandleDragUpdate, 
                HandleDragEnd
            );
    
            // Навешиваем манипулятор на корень визуального элемента
            element.Root.AddManipulator(dragManipulator);
        }
        public void HandleDragUpdate(HierarchyDragManipulator manipulator, Vector2 position)
        {
            UpdateGhostPosition(position);
            string targetId = GetTargetIdUnderPointer(position, manipulator.Element.Data.Id);
            UpdateHighlight(targetId);
        }

        public void HandleDragEnd(HierarchyDragManipulator manipulator, Vector2 position)
        {
            manipulator.Element.Root.style.opacity = 1.0f;
            ClearHighlight();

            if (_dragGhost != null)
            {
                _dragGhost.parent?.Remove(_dragGhost);
                _dragGhost = null;
            }

            if (position != Vector2.zero)
            {
                string targetId = GetTargetIdUnderPointer(position, manipulator.Element.Data.Id);
                OnItemMoveRequested?.Invoke(manipulator.Element.Data.Id, targetId);
            }
        }

        private string GetTargetIdUnderPointer(Vector2 position, string draggedId)
        {
            _pickList.Clear();
            _scrollView.panel.PickAll(position, _pickList);

            foreach (var picked in _pickList)
            {
                string id = FindTargetIdRecursive(picked);
                if (!string.IsNullOrEmpty(id) && id != draggedId) return id;
            }
            return null;
        }

        private string FindTargetIdRecursive(VisualElement element)
        {
            var current = element;
            while (current != null)
            {
                if (current.userData is string id && _elements.ContainsKey(id)) return id;
                current = current.parent;
            }
            return null;
        }

        private void UpdateHighlight(string targetId)
        {
            if (_currentDragTarget != null && _currentDragTarget.Data.Id != targetId) ClearHighlight();

            if (!string.IsNullOrEmpty(targetId) && _elements.TryGetValue(targetId, out var newTarget))
            {
                _currentDragTarget = newTarget;
                _currentDragTarget.Root.style.borderBottomWidth = 2;
                _currentDragTarget.Root.style.borderBottomColor = Color.cyan;
            }
        }

        private void ClearHighlight()
        {
            if (_currentDragTarget != null)
            {
                _currentDragTarget.Root.style.borderBottomWidth = StyleKeyword.Null;
                _currentDragTarget.Root.style.borderBottomColor = StyleKeyword.Null;
                _currentDragTarget = null;
            }
        }

        private void UpdateGhostPosition(Vector2 position)
        {
            if (_dragGhost == null) return;
            _dragGhost.style.left = position.x + 10;
            _dragGhost.style.top = position.y + 10;
        }
    }
}