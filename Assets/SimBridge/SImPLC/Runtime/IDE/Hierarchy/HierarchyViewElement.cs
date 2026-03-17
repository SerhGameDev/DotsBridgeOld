using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public abstract class HierarchyViewElement : IDisposable
    {
        // USS класс для визуального отображения выделенного состояния
        private const string SelectedUssClassName = "hierarchy-item-selected";

        public VisualElement Root { get; }
        public IHierarchyItemData Data { get; }

        public event Action<HierarchyViewElement> OnSelected;
        public event Action<HierarchyViewElement, Vector2> OnContextRequested;

        protected HierarchyViewElement(VisualElement rootElement, IHierarchyItemData data)
        {
            Root = rootElement;
            Data = data;
            Root.userData = data.Id;
            
            Root.RegisterCallback<ClickEvent>(OnClick);
            Root.RegisterCallback<PointerDownEvent>(OnPointerDownRightClick);
        }

        public abstract void SetName(string name);
        public virtual void SetExecutionOrder(int order){}
        private void OnPointerDownRightClick(PointerDownEvent evt)
        {
            // button == 1 означает клик правой кнопкой мыши
            if (evt.button == 1)
            {
                Vector2 panelPosition = Root.LocalToWorld(evt.localPosition);
                OnContextRequested?.Invoke(this, panelPosition);
                evt.StopPropagation();
            }
        }

        // Управление визуальным состоянием выделения
        public void SetSelectedState(bool isSelected)
        {
            if (isSelected)
                Root.AddToClassList(SelectedUssClassName);
            else
                Root.RemoveFromClassList(SelectedUssClassName);
        }

        private void OnClick(ClickEvent evt)
        {
            OnSelected?.Invoke(this);
            evt.StopPropagation();
        }

        private void OnContextClick(ContextClickEvent evt)
        {
            Vector2 panelPosition = Root.LocalToWorld(evt.localMousePosition);
            OnContextRequested?.Invoke(this, panelPosition);
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