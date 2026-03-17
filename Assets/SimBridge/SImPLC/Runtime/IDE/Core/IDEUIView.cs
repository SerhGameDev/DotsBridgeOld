using System;
using UnityEngine.UIElements;

namespace IDE
{
    public class IDEUIView : IDisposable
    {
        public HierarchyModelView HierarchyView { get; }
        private const string ContainerHierarchy = "hierarchy-container";
        private readonly VisualElement _hierarchyWindowRoot;

        public IDEUIView(
            VisualElement rootContainer, 
            VisualTreeAsset hierarchyWindowTemplate, 
            VisualTreeAsset fileTemplate, 
            VisualTreeAsset folderTemplate)
        {
            _hierarchyWindowRoot = hierarchyWindowTemplate.Instantiate();
            rootContainer.Q<VisualElement>(ContainerHierarchy).Add(_hierarchyWindowRoot);

            HierarchyView = new HierarchyModelView(_hierarchyWindowRoot, fileTemplate, folderTemplate);
        }

        public void Dispose()
        {
            HierarchyView?.Dispose();
            
            if (_hierarchyWindowRoot != null && _hierarchyWindowRoot.parent != null)
            {
                _hierarchyWindowRoot.parent.Remove(_hierarchyWindowRoot);
            }
        }
    }
}