using System.Collections.Generic;

namespace IDE
{
    public class HierarchySelectionModel
    {
        private readonly Dictionary<string, HierarchyViewElement> _elements;
        private string _currentSelectedId;

        public string CurrentSelectedId => _currentSelectedId;

        public HierarchySelectionModel(Dictionary<string, HierarchyViewElement> elements)
        {
            _elements = elements;
        }

        public void Select(string id)
        {
            if (_currentSelectedId == id) return;

            if (!string.IsNullOrEmpty(_currentSelectedId) && _elements.TryGetValue(_currentSelectedId, out var prev))
            {
                prev.SetSelectedState(false);
            }

            if (_elements.TryGetValue(id, out var current))
            {
                current.SetSelectedState(true);
                _currentSelectedId = id;
            }
        }

        public void Clear()
        {
            if (!string.IsNullOrEmpty(_currentSelectedId) && _elements.TryGetValue(_currentSelectedId, out var prev))
            {
                prev.SetSelectedState(false);
            }
            _currentSelectedId = null;
        }
    }
}