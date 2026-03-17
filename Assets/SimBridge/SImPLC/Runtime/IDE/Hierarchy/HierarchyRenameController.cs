using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public class HierarchyRenameController
    {
        private readonly Dictionary<string, HierarchyViewElement> _elements;
        public event Action<string, string> OnRenameCommitted;

        public HierarchyRenameController(Dictionary<string, HierarchyViewElement> elements)
        {
            _elements = elements;
        }

        public void StartRename(string id)
        {
            if (!_elements.TryGetValue(id, out var element)) return;

            var label = element.Root.Q<Label>("file-name-label") ?? 
                        element.Root.Q<Label>(className: "unity-foldout__text");
            
            if (label == null) return;

            var textField = new TextField { value = element.Data.Name };
            ApplyStyles(textField, label);
            element.Root.Add(textField);

            textField.schedule.Execute(() => {
                textField.Focus();
                textField.SelectAll();
            }).StartingIn(10);

            bool isDone = false;
            
            // Локальная функция завершения
            Action commit = () => {
                if (isDone) return;
                isDone = true;
                if (textField.parent != null) element.Root.Remove(textField);
                
                if (!string.IsNullOrWhiteSpace(textField.value) && textField.value != element.Data.Name)
                    OnRenameCommitted?.Invoke(id, textField.value);
            };

            textField.RegisterCallback<FocusOutEvent>(e => commit());
            textField.RegisterCallback<KeyDownEvent>(e => {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) { commit(); e.StopPropagation(); }
                else if (e.keyCode == KeyCode.Escape) { isDone = true; element.Root.Remove(textField); e.StopPropagation(); }
            });
        }

        private void ApplyStyles(TextField field, Label targetLabel)
        {
            field.style.position = Position.Absolute;
            field.style.left = targetLabel.layout.xMin;
            field.style.top = targetLabel.layout.yMin;
            field.style.color = Color.black;
            field.style.width = Mathf.Max(120, targetLabel.layout.width + 50);
            field.style.height = targetLabel.layout.height > 0 ? targetLabel.layout.height : 20;
        }
    }
}