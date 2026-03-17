using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public class HierarchyModelView : IDisposable
    {
        // Хранилище данных и элементов
        private readonly Dictionary<string, HierarchyViewElement> _elements = new Dictionary<string, HierarchyViewElement>();

        // Модули-помощники
        private readonly HierarchyRenderer _renderer;
        private readonly HierarchySelectionModel _selectionModel;
        private readonly HierarchyDragAndDropController _dndController;
        private readonly HierarchyInteractionHandler _interactionHandler;

        // Проброс событий для внешней Модели (Hierarchy.cs)
        public event Action<string> OnItemSelected;
        public event Action<string, Vector2> OnItemContextRequested;
        public event Action<string, string> OnItemMoveRequested;

        public HierarchyModelView(VisualElement root, VisualTreeAsset fileTemplate, VisualTreeAsset folderTemplate)
        {
            var scrollView = root.Q<ScrollView>("hierarchy-scroll-view");

            // Инициализация модулей
            _renderer = new HierarchyRenderer(scrollView, fileTemplate, folderTemplate);
            _selectionModel = new HierarchySelectionModel(_elements);
            _dndController = new HierarchyDragAndDropController(scrollView, _elements);
            _interactionHandler = new HierarchyInteractionHandler();

            // Связывание событий
            _interactionHandler.OnItemSelected += id => OnItemSelected?.Invoke(id);
            _interactionHandler.OnContextRequested += (id, pos) => OnItemContextRequested?.Invoke(id, pos);
            _dndController.OnItemMoveRequested += (id, targetId) => OnItemMoveRequested?.Invoke(id, targetId);

            // Регистрация глобальных зон клика
            _interactionHandler.RegisterBackground(scrollView);
        }

        public void AddFile(IHierarchyItemData data, string parentFolderId = null)
        {
            var element = _renderer.CreateFileElement(data);
            InitializeAndAdd(element, parentFolderId);
        }

        public void AddFolder(IHierarchyItemData data, string parentFolderId = null)
        {
            var element = _renderer.CreateFolderElement(data);
            InitializeAndAdd(element, parentFolderId);
        }
        public void RegisterBackgroundContextTrigger(VisualElement element)
        {
            if (element != null)
            {
                _interactionHandler.RegisterBackground(element);
            }
        }
        private void InitializeAndAdd(HierarchyViewElement element, string parentId)
        {
            _elements[element.Data.Id] = element;
    
            // 1. Регистрация кликов и контекстного меню
            _interactionHandler.RegisterElement(element);
    
            // 2. ПОДКЛЮЧЕНИЕ ПЕРЕТАСКИВАНИЯ (Раскомментировать)
            _dndController.Attach(element); 

            // 3. Отрисовка в дереве
            _elements.TryGetValue(parentId ?? "", out var parent);
            _renderer.AddToTree(element, parent);
        }

        public void SelectElement(string id) => _selectionModel.Select(id);
        public void ClearSelection() => _selectionModel.Clear();
        public void ReorderElement(string drgId, string trgId) => _renderer.Reorder(_elements[drgId], _elements[trgId]);
        
        public void MoveElement(string id, string newParentId, string oldParentId)
        {
            if (!_elements.TryGetValue(id, out var el)) return;
            
            _elements.TryGetValue(newParentId ?? "", out var newParent);
            _renderer.RemoveFromTree(el);
            _renderer.AddToTree(el, newParent);
        }

        public void Dispose()
        {
            foreach (var el in _elements.Values) el.Dispose();
            _elements.Clear();
        }
    }
}