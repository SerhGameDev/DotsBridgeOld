using System.Collections.Generic;
using UnityEngine.UIElements;

namespace IDE
{
    public class HierarchyRenderer
    {
        private readonly ScrollView _scrollView;
        private readonly VisualTreeAsset _fileTemplate;
        private readonly VisualTreeAsset _folderTemplate;

        public HierarchyRenderer(ScrollView scrollView, VisualTreeAsset fileTemplate, VisualTreeAsset folderTemplate)
        {
            _scrollView = scrollView;
            _fileTemplate = fileTemplate;
            _folderTemplate = folderTemplate;
        }

        // Создает визуальный элемент файла
        public HierarchyViewElement CreateFileElement(IHierarchyItemData data)
        {
            var visual = _fileTemplate.Instantiate();
            return new HierarchyViewElementFile(visual, data);
        }

        // Создает визуальный элемент папки
        public HierarchyViewElement CreateFolderElement(IHierarchyItemData data)
        {
            var visual = _folderTemplate.Instantiate();
            return new HierarchyViewElementFolder(visual, data);
        }

        // Помещает элемент в дерево (в корень или в папку)
        public void AddToTree(HierarchyViewElement element, HierarchyViewElement parent = null)
        {
            if (parent is HierarchyViewElementFolder folder)
            {
                folder.AddChild(element);
            }
            else
            {
                _scrollView.contentContainer.Add(element.Root);
            }
        }

        // Удаляет элемент из UI
        public void RemoveFromTree(HierarchyViewElement element)
        {
            if (element.Root.parent != null)
            {
                element.Root.parent.Remove(element.Root);
            }
        }

        // Визуальная перестановка внутри одного контейнера
        public void Reorder(HierarchyViewElement dragged, HierarchyViewElement target)
        {
            var parent = dragged.Root.parent;
            if (parent != null && parent == target.Root.parent)
            {
                int targetIndex = parent.IndexOf(target.Root);
                parent.Insert(targetIndex, dragged.Root);
            }
        }
    }
}