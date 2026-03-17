using System;
using System.Collections.Generic;
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
        private string _currentSelectedId;

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

            if (!string.IsNullOrEmpty(parentFolderId) && 
                _elements.TryGetValue(parentFolderId, out var parentElement) && 
                parentElement is HierarchyViewElementFolder folder)
            {
                folder.AddChild(element);
            }
            else
            {
                // Иначе добавляем в корень ScrollView
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