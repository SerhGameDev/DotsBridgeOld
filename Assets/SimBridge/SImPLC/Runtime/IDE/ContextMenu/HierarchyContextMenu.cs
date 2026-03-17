using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public class HierarchyContextMenu : BaseContextMenu
    {
        private readonly Hierarchy _hierarchyModel;
        private string _currentTargetId;

        public HierarchyContextMenu(VisualElement root, VisualTreeAsset menuTemplate, VisualTreeAsset itemTemplate, Hierarchy hierarchyModel) 
            : base(root, menuTemplate, itemTemplate)
        {
            _hierarchyModel = hierarchyModel;
        }

        public void ShowContextMenu(Vector2 position, string targetId)
        {
            _currentTargetId = targetId;
            Show(position);
        }

        protected override List<ContextMenuAction> GetActions()
        {
            var actions = new List<ContextMenuAction>
            {
                new ContextMenuAction("Create File", () => _hierarchyModel.CreateFile("New File", _currentTargetId)),
                new ContextMenuAction("Create Folder", () => _hierarchyModel.CreateFolder("New Folder", _currentTargetId))
            };

            // Если кликнули по конкретному элементу (не по фону) — добавляем опции управления
            if (!string.IsNullOrEmpty(_currentTargetId))
            {
                actions.Add(new ContextMenuAction("Rename", () => _hierarchyModel.TriggerRename(_currentTargetId)));
                actions.Add(new ContextMenuAction("Delete", () => _hierarchyModel.RemoveItem(_currentTargetId)));
            }

            return actions;
        }
    }
}