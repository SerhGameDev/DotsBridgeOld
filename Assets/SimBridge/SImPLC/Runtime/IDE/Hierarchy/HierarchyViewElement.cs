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
        public event Action<HierarchyViewElement, string> OnRenameCommitted;

        protected HierarchyViewElement(VisualElement rootElement, IHierarchyItemData data)
        {
            Root = rootElement;
            Data = data;
            Root.userData = data.Id;
            
            Root.RegisterCallback<ClickEvent>(OnClick);
            Root.RegisterCallback<PointerDownEvent>(OnPointerDownRightClick);
        }
        public void StartRename()
        {
            // Пытаемся найти Label (для файла ищем по имени, для папки Foldout использует встроенный класс Unity)
            var label = Root.Q<Label>("file-name-label") ?? Root.Q<Label>(className: "unity-foldout__text");
            if (label == null) return;

            // Создаем TextField на лету
            var textField = new TextField { value = Data.Name };
            textField.style.position = Position.Absolute;
            textField.style.left = label.layout.xMin;
            textField.style.top = label.layout.yMin;
            textField.style.width = Mathf.Max(120, label.layout.width + 20);
            textField.style.height = label.layout.height > 0 ? label.layout.height : 20;

            Root.Add(textField);
    
            // Фокусируемся с задержкой, чтобы элемент успел отрендериться в кадре
            textField.schedule.Execute(() =>
            {
                textField.Focus();
                textField.SelectAll();
            }).StartingIn(10);

            bool isCommitted = false;

            void Commit()
            {
                if (isCommitted) return;
                isCommitted = true;
        
                if (textField.parent != null) Root.Remove(textField);

                if (!string.IsNullOrWhiteSpace(textField.value) && textField.value != Data.Name)
                {
                    OnRenameCommitted?.Invoke(this, textField.value);
                }
            }

            textField.RegisterCallback<FocusOutEvent>(e => Commit());
            textField.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    Commit();
                    e.StopPropagation();
                }
                else if (e.keyCode == KeyCode.Escape) // Отмена
                {
                    isCommitted = true; 
                    Root.Remove(textField);
                    e.StopPropagation();
                }
            });
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