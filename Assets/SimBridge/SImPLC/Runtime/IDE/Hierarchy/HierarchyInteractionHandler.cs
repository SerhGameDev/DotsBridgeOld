using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public class HierarchyInteractionHandler
    {
        public event Action<string> OnItemSelected;
        public event Action<string, Vector2> OnContextRequested;

        public void RegisterElement(HierarchyViewElement element)
        {
            element.OnSelected += id => OnItemSelected?.Invoke(element.Data.Id);
            element.OnContextRequested += (el, pos) => OnContextRequested?.Invoke(el.Data.Id, pos);
        }

        public void RegisterBackground(VisualElement bgElement)
        {
            bgElement.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    Vector2 pos = bgElement.LocalToWorld(evt.localPosition);
                    OnContextRequested?.Invoke(null, pos);
                    evt.StopPropagation();
                }
            });
        }
    }
}