using System;
using System.Collections.Generic;

namespace IDE
{
    public class Hierarchy : IDisposable
    {
        private readonly HierarchyModelView _view;
        private readonly Dictionary<string, IHierarchyItemData> _items = new Dictionary<string, IHierarchyItemData>();
        
        private int _nextExecutionOrder = 1;

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
            
            return id;
        }

        public string CreateFolder(string name, string parentFolderId = null)
        {
            string id = Guid.NewGuid().ToString();
            var folderData = new HierarchyItemData(id, name, true);
            
            _items.Add(id, folderData);
            _view.AddFolder(folderData, parentFolderId);
            
            return id;
        }

        public void RemoveItem(string id)
        {
            if (_items.Remove(id))
            {
                _view.RemoveElement(id);
            }
        }

        private void HandleItemSelected(string id)
        {
            if (_items.TryGetValue(id, out var item) && !item.IsFolder)
            {
                OnFileOpened?.Invoke(id);
            }
        }

        private void HandleItemContextRequested(string id)
        {
            // Здесь в будущем будет вызов контекстного меню
        }

        public void Dispose()
        {
            _view.OnItemSelected -= HandleItemSelected;
            _view.OnItemContextRequested -= HandleItemContextRequested;
            _items.Clear();
        }
    }
}