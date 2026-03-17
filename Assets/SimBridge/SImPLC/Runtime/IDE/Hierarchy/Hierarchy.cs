using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDE
{
    public class Hierarchy : IDisposable
    {
        private readonly HierarchyModelView _view;
        private readonly Dictionary<string, IHierarchyItemData> _items = new Dictionary<string, IHierarchyItemData>();
        public event Action<string, string> OnItemMoved;
        private int _nextExecutionOrder = 1;
        public string CurrentSelectedId { get; private set; }

        public event Action<string> OnItemCreated;
        public event Action<string> OnItemRemoved;
        public event Action<string> OnSelectionChanged;
        public event Action<string, Vector2> OnItemContextRequested;
        public event Action<string> OnFileOpened;

        public Hierarchy(HierarchyModelView view)
        {
            _view = view;
            _view.OnItemMoveRequested += HandleItemMoveRequested;
            _view.OnItemRenamed += HandleItemRenamed;
            _view.OnItemSelected += HandleItemSelected;
            _view.OnItemContextRequested += HandleItemContextRequested;
        }

        public string CreateFile(string name, string parentFolderId = null)
        {
            string id = Guid.NewGuid().ToString();
            var fileData = new HierarchyItemData(id, name, false, parentFolderId, _nextExecutionOrder++);
            
            _items.Add(id, fileData);
            _view.AddFile(fileData, parentFolderId);
            
            OnItemCreated?.Invoke(id);
            return id;
        }
        public void TriggerRename(string id) => _view.StartRename(id);

        private void HandleItemRenamed(string id, string newName)
        {
            if (_items.TryGetValue(id, out var item) && item is HierarchyItemData data)
            {
                data.Name = newName;
                _view.UpdateItemName(id, newName);
            }
        }

        public void RemoveItem(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            RemoveItemRecursive(id);
        }

        private void RemoveItemRecursive(string id)
        {
            if (!_items.TryGetValue(id, out var item)) return;

            // Сначала находим и удаляем всех детей (если это папка)
            var childrenIds = new List<string>();
            foreach (var kvp in _items)
            {
                if (kvp.Value.ParentId == id) childrenIds.Add(kvp.Key);
            }

            foreach (var childId in childrenIds)
            {
                RemoveItemRecursive(childId);
            }

            // Удаляем сам элемент
            _items.Remove(id);
            _view.RemoveElement(id);
            
            if (CurrentSelectedId == id)
            {
                CurrentSelectedId = null;
                OnSelectionChanged?.Invoke(null);
            }
            
            OnItemRemoved?.Invoke(id);
        }
        public string CreateFolder(string name, string parentFolderId = null)
        {
            string id = Guid.NewGuid().ToString();
            var folderData = new HierarchyItemData(id, name, true, parentFolderId);
            
            _items.Add(id, folderData);
            _view.AddFolder(folderData, parentFolderId);
            
            OnItemCreated?.Invoke(id);
            return id;
        }

        private void HandleItemSelected(string id)
        {
            if (CurrentSelectedId != id)
            {
                CurrentSelectedId = id;
                _view.SelectElement(id);
                OnSelectionChanged?.Invoke(id);
            }

            if (_items.TryGetValue(id, out var item) && !item.IsFolder)
            {
                OnFileOpened?.Invoke(id);
            }
        }

        private void HandleItemContextRequested(string id, Vector2 position)
        {
            OnItemContextRequested?.Invoke(id, position);
        }
        private void HandleItemMoveRequested(string id, string targetId)
        {
            if (id == targetId || string.IsNullOrEmpty(id)) return;

            string newParentId = null;
            bool isDroppedOnSibling = false;

            if (!string.IsNullOrEmpty(targetId) && _items.TryGetValue(targetId, out var targetItem))
            {
                if (targetItem.IsFolder)
                {
                    newParentId = targetId;
                }
                else
                {
                    newParentId = targetItem.ParentId; 
                    isDroppedOnSibling = true; // Мы бросили файл на другой файл
                }
            }

            if (_items.TryGetValue(id, out var draggedItem))
            {
                string oldParentId = draggedItem.ParentId;

                // Если бросили в ту же папку на соседа — переставляем
                if (oldParentId == newParentId && isDroppedOnSibling)
                {
                    _view.ReorderElement(id, targetId);
                }
                // Если папки разные — переносим
                else if (oldParentId != newParentId)
                {
                    MoveItem(id, newParentId);
                }
            }
        }

        public void MoveItem(string id, string newParentId)
        {
            if (_items.TryGetValue(id, out var item) && item is HierarchyItemData data)
            {
                string oldParentId = data.ParentId;
                if (oldParentId == newParentId) return;

                if (item.IsFolder && IsDescendantOf(newParentId, id)) return;

                data.ParentId = newParentId;
                _view.MoveElement(id, newParentId, oldParentId);
        
                OnItemMoved?.Invoke(id, newParentId);
            }
        }

        private bool IsDescendantOf(string potentialChildId, string ancestorId)
        {
            string currentId = potentialChildId;
            while (!string.IsNullOrEmpty(currentId))
            {
                if (currentId == ancestorId) return true;
        
                if (_items.TryGetValue(currentId, out var item))
                    currentId = item.ParentId;
                else
                    break;
            }
            return false;
        }
        public void Dispose()
        {
            _view.OnItemRenamed -= HandleItemRenamed;
            _view.OnItemMoveRequested -= HandleItemMoveRequested;
            _view.OnItemSelected -= HandleItemSelected;
            _view.OnItemContextRequested -= HandleItemContextRequested;
            _items.Clear();
        }
    }
}