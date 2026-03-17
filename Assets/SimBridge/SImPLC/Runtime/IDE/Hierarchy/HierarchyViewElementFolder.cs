using System.Collections.Generic;
using UnityEngine.UIElements;

namespace IDE
{
    public class HierarchyViewElementFolder : HierarchyViewElement
    {
        // Корневой элемент папки с возможностью сворачивания
        private const string FoldoutName = "folder-foldout"; 
        // Контейнер для вложенных файлов и папок
        private const string ContentContainerName = "folder-content-container"; 

        private readonly Foldout _foldout;
        private readonly VisualElement _contentContainer;
        private readonly List<HierarchyViewElement> _children = new List<HierarchyViewElement>();

        public HierarchyViewElementFolder(VisualElement rootElement, IHierarchyItemData data) 
            : base(rootElement, data)
        {
            _foldout = Root.Q<Foldout>(FoldoutName);
            _contentContainer = Root.Q<VisualElement>(ContentContainerName);
            
            SetName(data.Name);
        }

        public override void SetName(string name)
        {
            if (_foldout != null) _foldout.text = name;
        }

        public void AddChild(HierarchyViewElement childElement)
        {
            _children.Add(childElement);
            _contentContainer?.Add(childElement.Root);
        }

        public void RemoveChild(HierarchyViewElement childElement)
        {
            _children.Remove(childElement);
            _contentContainer?.Remove(childElement.Root);
        }
    }
}