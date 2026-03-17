using System;
using UnityEngine.UIElements;

namespace IDE
{
    public abstract class HierarchyViewElement : IDisposable
    {
        public VisualElement Root { get; }
        public IHierarchyItemData Data { get; }

        public event Action<HierarchyViewElement> OnSelected;
        public event Action<HierarchyViewElement> OnContextRequested;

        protected HierarchyViewElement(VisualElement rootElement, IHierarchyItemData data)
        {
            Root = rootElement;
            Data = data;

            Root.RegisterCallback<ClickEvent>(OnClick);
            Root.RegisterCallback<ContextClickEvent>(OnContextClick);
        }

        public abstract void SetName(string name);
        
        public abstract void SetExecutionOrder(int order);

        private void OnClick(ClickEvent evt)
        {
            OnSelected?.Invoke(this);
            evt.StopPropagation();
        }

        private void OnContextClick(ContextClickEvent evt)
        {
            OnContextRequested?.Invoke(this);
            evt.StopPropagation();
        }

        public virtual void Dispose()
        {
            if (Root != null)
            {
                Root.UnregisterCallback<ClickEvent>(OnClick);
                Root.UnregisterCallback<ContextClickEvent>(OnContextClick);
            }
        }
    }
}