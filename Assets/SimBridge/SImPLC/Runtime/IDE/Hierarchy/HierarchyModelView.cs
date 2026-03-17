using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public class HierarchyModelView : IDisposable
    {
        private const string ScrollViewName = "hierarchy-scroll-view";

        private readonly ScrollView _scrollView;
        private readonly VisualTreeAsset _fileTemplate;
        private readonly VisualTreeAsset _folderTemplate;

        private readonly Dictionary<string, HierarchyViewElement> _elements = new Dictionary<string, HierarchyViewElement>();

        public event Action<string> OnItemSelected;
        public event Action<string> OnItemContextRequested;
        public event Action<string, string> OnItemMoveRequested;
        private string _currentSelectedId;
        private VisualElement _dragGhost;
        

        public HierarchyModelView(VisualElement root, VisualTreeAsset fileTemplate, VisualTreeAsset folderTemplate)
        {
            _fileTemplate = fileTemplate;
            _folderTemplate = folderTemplate;
            
            _scrollView = root.Q<ScrollView>(ScrollViewName);
        }

        public void SelectElement(string id)
        {
            // Снимаем выделение с предыдущего элемента
            if (!string.IsNullOrEmpty(_currentSelectedId) && _elements.TryGetValue(_currentSelectedId, out var prevElement))
            {
                prevElement.SetSelectedState(false);
            }

            // Выделяем новый элемент
            if (_elements.TryGetValue(id, out var newElement))
            {
                newElement.SetSelectedState(true);
                _currentSelectedId = id;
            }
        }

        public void ClearSelection()
        {
            if (!string.IsNullOrEmpty(_currentSelectedId) && _elements.TryGetValue(_currentSelectedId, out var prevElement))
            {
                prevElement.SetSelectedState(false);
                _currentSelectedId = null;
            }
        }
        public void AddFile(IHierarchyItemData data, string parentFolderId = null)
        {
            var fileVisual = _fileTemplate.Instantiate();
            var fileElement = new HierarchyViewElementFile(fileVisual, data);
            
            AddElementToTree(fileElement, parentFolderId);
        }

        public void AddFolder(IHierarchyItemData data, string parentFolderId = null)
        {
            var folderVisual = _folderTemplate.Instantiate();
            var folderElement = new HierarchyViewElementFolder(folderVisual, data);
            
            AddElementToTree(folderElement, parentFolderId);
        }

        private void AddElementToTree(HierarchyViewElement element, string parentFolderId)
        {
            _elements[element.Data.Id] = element;
            
            element.OnSelected += HandleItemSelected;
            element.OnContextRequested += HandleItemContextRequested;
            var dragManipulator = new HierarchyDragManipulator(element, HandleDragStart, HandleDragUpdate, HandleDragEnd);
            element.Root.AddManipulator(dragManipulator);
            if (!string.IsNullOrEmpty(parentFolderId) && 
                _elements.TryGetValue(parentFolderId, out var parentElement) && 
                parentElement is HierarchyViewElementFolder folder)
            {
                folder.AddChild(element);
            }
            else
            {
                _scrollView.Add(element.Root);
            }
        }
        private void HandleDragStart(HierarchyDragManipulator manipulator, Vector2 position)
        {
            var original = manipulator.Element.Root;
            original.style.opacity = 0.3f;

            // Создаем "призрака" — визуальную копию перетаскиваемого элемента
            _dragGhost = new Label(manipulator.Element.Data.Name); // Можно инстанцировать UXML для красоты
            _dragGhost.style.position = Position.Absolute;
            _dragGhost.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            _dragGhost.style.borderBottomColor = _dragGhost.style.borderTopColor = Color.cyan;
            _dragGhost.style.borderLeftWidth = _dragGhost.style.borderRightWidth = 2;
            _dragGhost.style.paddingLeft = _dragGhost.style.paddingRight = 5;
            _dragGhost.pickingMode = PickingMode.Ignore; // Важно: чтобы Pick не выбирал самого призрака

            _scrollView.panel.visualTree.Add(_dragGhost);
            UpdateGhostPosition(position);
        }
        private void HandleDragEnd(HierarchyDragManipulator manipulator, Vector2 position)
        {
            var elementRoot = manipulator.Element.Root;
            elementRoot.style.opacity = 1.0f;

            if (_dragGhost != null)
            {
                _dragGhost.parent?.Remove(_dragGhost);
                _dragGhost = null;
            }

            if (position == Vector2.zero) return;

            // ВАЖНО: Полностью скрываем элемент, чтобы луч точно пролетел сквозь все 
            // дочерние Label и иконки, которые иначе перехватили бы panel.Pick.
            var initialDisplay = elementRoot.style.display;
            elementRoot.style.display = DisplayStyle.None;

            // Поиск цели (папки или файла) под курсором
            var pickedElement = _scrollView.panel.Pick(position);
            string targetId = FindTargetIdRecursive(pickedElement);

            // Возвращаем видимость элемента
            elementRoot.style.display = initialDisplay;

            // Для отладки (чтобы убедиться, что цель найдена верно)
             Debug.Log($"[DragAndDrop] Dragged: {manipulator.Element.Data.Id}, Target: {targetId}");

            OnItemMoveRequested?.Invoke(manipulator.Element.Data.Id, targetId);
        }
        public void MoveElement(string id, string newParentId, string oldParentId)
        {
            if (!_elements.TryGetValue(id, out var element)) return;

            if (!string.IsNullOrEmpty(oldParentId) && _elements.TryGetValue(oldParentId, out var oldParent) && oldParent is HierarchyViewElementFolder oldFolder)
            {
                oldFolder.RemoveChild(element);
            }
            else if (element.Root.parent != null)
            {
                element.Root.parent.Remove(element.Root);
            }

            if (!string.IsNullOrEmpty(newParentId) && _elements.TryGetValue(newParentId, out var newParent) && newParent is HierarchyViewElementFolder newFolder)
            {
                newFolder.AddChild(element);
            }
            else
            {
                _scrollView.Add(element.Root);
            }
        }
        public void RemoveElement(string id)
        {
            if (_elements.TryGetValue(id, out var element))
            {
                // Отписываемся от событий, чтобы избежать утечек памяти
                element.OnSelected -= HandleItemSelected;
                element.OnContextRequested -= HandleItemContextRequested;
                
                // Удаляем визуальный элемент из родительского контейнера
                if (element.Root.parent != null)
                {
                    element.Root.parent.Remove(element.Root);
                }

                _elements.Remove(id);
                element.Dispose();
            }
        }
        

private void HandleDragUpdate(HierarchyDragManipulator manipulator, Vector2 position)
{
    UpdateGhostPosition(position);
}

private void UpdateGhostPosition(Vector2 position)
{
    if (_dragGhost == null) return;
    
    // Смещение призрака чуть в сторону от курсора
    _dragGhost.style.left = position.x + 10;
    _dragGhost.style.top = position.y + 10;
}


private string FindTargetIdRecursive(VisualElement element)
{
    var current = element;
    while (current != null)
    {
        if (current.userData is string id && _elements.ContainsKey(id))
            return id;
        current = current.parent;
    }
    return null;
}
        private void HandleItemSelected(HierarchyViewElement element)
        {
            OnItemSelected?.Invoke(element.Data.Id);
        }

        private void HandleItemContextRequested(HierarchyViewElement element)
        {
            OnItemContextRequested?.Invoke(element.Data.Id);
        }

        public void Dispose()
        {
            foreach (var element in _elements.Values)
            {
                element.OnSelected -= HandleItemSelected;
                element.OnContextRequested -= HandleItemContextRequested;
                element.Dispose();
            }
            _elements.Clear();
        }
    }
}