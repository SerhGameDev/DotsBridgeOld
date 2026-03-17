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
        public event Action<string, Vector2> OnItemContextRequested;
        public event Action<string, string> OnItemMoveRequested;
        private string _currentSelectedId;
        private HierarchyViewElement _currentDragTargetElement;
        private VisualElement _dragGhost;
        
        private readonly List<VisualElement> _pickList = new List<VisualElement>();
        
        
        public HierarchyModelView(VisualElement root, VisualTreeAsset fileTemplate, VisualTreeAsset folderTemplate)
        {
            _fileTemplate = fileTemplate;
            _folderTemplate = folderTemplate;
            
            _scrollView = root.Q<ScrollView>(ScrollViewName);
            _scrollView.RegisterCallback<ContextClickEvent>(OnBackgroundContextClick);
        }
        private void OnBackgroundContextClick(ContextClickEvent evt)
        {
            Vector2 panelPosition = _scrollView.LocalToWorld(evt.localMousePosition);
            OnItemContextRequested?.Invoke(null, panelPosition);
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
                _scrollView.contentContainer.Add(element.Root);
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
        public void ReorderElement(string draggedId, string targetId)
        {
            if (_elements.TryGetValue(draggedId, out var draggedEl) && 
                _elements.TryGetValue(targetId, out var targetEl))
            {
                var parent = draggedEl.Root.parent;
        
                if (parent != null && parent == targetEl.Root.parent)
                {
                    int targetIndex = parent.IndexOf(targetEl.Root);
                    parent.Insert(targetIndex, draggedEl.Root);
                }
            }
        }
        private void HandleDragEnd(HierarchyDragManipulator manipulator, Vector2 position)
        {
            manipulator.Element.Root.style.opacity = 1.0f;

            ClearDragTargetHighlight();

            if (_dragGhost != null)
            {
                _dragGhost.parent?.Remove(_dragGhost);
                _dragGhost = null;
            }

            if (position == Vector2.zero) return;

            // Находим цель для перемещения
            string targetId = GetTargetIdUnderPointer(position, manipulator.Element.Data.Id);

            OnItemMoveRequested?.Invoke(manipulator.Element.Data.Id, targetId);
        }
        public void MoveElement(string id, string newParentId, string oldParentId)
        {
            if (!_elements.TryGetValue(id, out var element)) return;

            if (!string.IsNullOrEmpty(oldParentId) && _elements.TryGetValue(oldParentId, out var oldParent) && oldParent is HierarchyViewElementFolder oldFolder)
            {
                oldFolder.RemoveChild(element);
            }
            else
            {
                _scrollView.contentContainer.Remove(element.Root);
            }

            if (!string.IsNullOrEmpty(newParentId) && _elements.TryGetValue(newParentId, out var newParent) && newParent is HierarchyViewElementFolder newFolder)
            {
                newFolder.AddChild(element);
            }
            else
            {
                _scrollView.contentContainer.Add(element.Root);
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
            string targetId = GetTargetIdUnderPointer(position, manipulator.Element.Data.Id);
            UpdateDragTargetHighlight(targetId);
        }
        private string GetTargetIdUnderPointer(Vector2 position, string draggedElementId)
        {
            _pickList.Clear();
    
            // Передаем координаты панели и список для заполнения
            _scrollView.panel.PickAll(position, _pickList);
    
            foreach (var picked in _pickList)
            {
                string id = FindTargetIdRecursive(picked);
        
                // Нашли ID, который не принадлежит перетаскиваемому объекту
                if (!string.IsNullOrEmpty(id) && id != draggedElementId)
                {
                    return id;
                }
            }
            return null;
        }
        private void UpdateDragTargetHighlight(string targetId)
        {
            // Если мышь ушла с предыдущей цели — стираем обводку
            if (_currentDragTargetElement != null && _currentDragTargetElement.Data.Id != targetId)
            {
                ClearDragTargetHighlight();
            }

            // Если навели на новую цель — рисуем обводку
            if (!string.IsNullOrEmpty(targetId) && _elements.TryGetValue(targetId, out var newTarget))
            {
                _currentDragTargetElement = newTarget;
        
                // Включаем циановую полосу снизу
                _currentDragTargetElement.Root.style.borderBottomWidth = 2;
                _currentDragTargetElement.Root.style.borderBottomColor = Color.cyan;
            }
        }
        private void ClearDragTargetHighlight()
        {
            if (_currentDragTargetElement != null)
            {
                _currentDragTargetElement.Root.style.borderBottomWidth = StyleKeyword.Null;
                _currentDragTargetElement.Root.style.borderBottomColor = StyleKeyword.Null;
                _currentDragTargetElement = null;
            }
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

        private void HandleItemContextRequested(HierarchyViewElement element, Vector2 position)
        {
            OnItemContextRequested?.Invoke(element.Data.Id, position);
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