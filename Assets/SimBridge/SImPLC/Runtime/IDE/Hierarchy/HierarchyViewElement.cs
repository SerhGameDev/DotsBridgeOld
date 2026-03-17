using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public abstract class HierarchyViewElement : IDisposable
    {
        private const string SelectedUssClassName = "hierarchy-item-selected";

        public VisualElement Root { get; }
        public IHierarchyItemData Data { get; }

        public event Action<HierarchyViewElement> OnSelected;
        public event Action<HierarchyViewElement, Vector2> OnContextRequested;
        public event Action<HierarchyViewElement> OnDoubleClicked;

        protected HierarchyViewElement(VisualElement rootElement, IHierarchyItemData data)
        {
            Root = rootElement;
            Data = data;
            Root.userData = data.Id;
            
            Root.RegisterCallback<ClickEvent>(OnClick);
            Root.RegisterCallback<PointerDownEvent>(OnPointerDownRightClick);
        }

        public abstract void SetName(string name);
        public virtual void SetExecutionOrder(int order) { }

        public void SetSelectedState(bool isSelected)
        {
            if (isSelected) Root.AddToClassList(SelectedUssClassName);
            else Root.RemoveFromClassList(SelectedUssClassName);
        }

        private void OnClick(ClickEvent evt)
        {
            if (evt.clickCount == 1)
            {
                OnSelected?.Invoke(this);
            }
            else if (evt.clickCount == 2)
            {
                OnDoubleClicked?.Invoke(this);
            }
            evt.StopPropagation();
        }

        private void OnPointerDownRightClick(PointerDownEvent evt)
        {
            if (evt.button == 1)
            {
                Vector2 panelPosition = Root.LocalToWorld(evt.localPosition);
                OnContextRequested?.Invoke(this, panelPosition);
                evt.StopPropagation();
            }
        }

        public virtual void Dispose()
        {
            Root?.UnregisterCallback<ClickEvent>(OnClick);
            Root?.UnregisterCallback<PointerDownEvent>(OnPointerDownRightClick);
        }
    }
}