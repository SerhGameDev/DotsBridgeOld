using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace IDE
{
    public class HierarchyModelView : IDisposable
    {
        // Контейнер для скроллинга списка иерархии
        private const string ScrollViewName = "hierarchy-scroll-view";

        private readonly ScrollView _scrollView;
        private readonly VisualTreeAsset _fileTemplate;
        private readonly VisualTreeAsset _folderTemplate;

        // Хранение всех элементов по Id для быстрого доступа (O(1) при поиске/удалении)
        private readonly Dictionary<string, HierarchyViewElement> _elements = new Dictionary<string, HierarchyViewElement>();

        // События, которые будут слушаться главной Моделью (IdeWorkspace или Hierarchy Model)
        public event Action<string> OnItemSelected;
        public event Action<string> OnItemContextRequested;

        public HierarchyModelView(VisualElement root, VisualTreeAsset fileTemplate, VisualTreeAsset folderTemplate)
        {
            _fileTemplate = fileTemplate;
            _folderTemplate = folderTemplate;
            
            _scrollView = root.Q<ScrollView>(ScrollViewName);
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
            
            // Подписываемся на события дочернего элемента
            element.OnSelected += HandleItemSelected;
            element.OnContextRequested += HandleItemContextRequested;

            // Если указан родитель и он является папкой — добавляем внутрь папки
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