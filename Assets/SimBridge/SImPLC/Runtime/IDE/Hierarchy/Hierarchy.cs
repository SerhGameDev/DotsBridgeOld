using System;
using System.Collections.Generic;

namespace IDE
{
    public class Hierarchy : IDisposable
    {
        private readonly HierarchyModelView _view;
        private readonly Dictionary<string, IHierarchyItemData> _items = new Dictionary<string, IHierarchyItemData>();
        
        private int _nextExecutionOrder = 1;
        public string CurrentSelectedId { get; private set; }

        public event Action<string> OnItemCreated;
        public event Action<string> OnItemRemoved;
        public event Action<string> OnSelectionChanged;
        public event Action<string> OnItemContextRequested;
        public event Action<string> OnFileOpened;

        public Hierarchy(HierarchyModelView view)
        {
            _view = view;
            
            _view.OnItemSelected += HandleItemSelected;
            _view.OnItemContextRequested += HandleItemContextRequested;
        }

        public string CreateFile(string name, string parentFolderId = null)
        {
            string id = Guid.NewGuid().ToString();
            var fileData = new HierarchyItemData(id, name, false, _nextExecutionOrder++);
            
            _items.Add(id, fileData);
            _view.AddFile(fileData, parentFolderId);
            
            OnItemCreated?.Invoke(id);
            return id;
        }

        public string CreateFolder(string name, string parentFolderId = null)
        {
            string id = Guid.NewGuid().ToString();
            var folderData = new HierarchyItemData(id, name, true);
            
            _items.Add(id, folderData);
            _view.AddFolder(folderData, parentFolderId);
            
            OnItemCreated?.Invoke(id);
            return id;
        }

        public void RemoveItem(string id)
        {
            if (_items.Remove(id))
            {
                _view.RemoveElement(id);
                
                if (CurrentSelectedId == id)
                {
                    CurrentSelectedId = null;
                    OnSelectionChanged?.Invoke(null);
                }
                
                OnItemRemoved?.Invoke(id);
            }
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

        private void HandleItemContextRequested(string id)
        {
            OnItemContextRequested?.Invoke(id);
        }

        public void Dispose()
        {
            _view.OnItemSelected -= HandleItemSelected;
            _view.OnItemContextRequested -= HandleItemContextRequested;
            _items.Clear();
        }
    }
}