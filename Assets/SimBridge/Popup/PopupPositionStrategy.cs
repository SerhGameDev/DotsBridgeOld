using UnityEngine;
using UnityEngine.UIElements;

namespace DotsBridge.UI.Positioning
{
    public interface IPopupPositionStrategy
    {
        void OnCreated(VisualElement popup, VisualElement root);
        void ApplyPosition(VisualElement popup);
        void OnDestroyed(VisualElement popup, VisualElement root);
    }

    public class MouseCursorStrategy : IPopupPositionStrategy
    {
        public Vector2 Offset = new Vector2(15, 15);

        public void OnCreated(VisualElement popup, VisualElement root)
        {
            popup.style.position = Position.Absolute;
            root.Add(popup);
        }

        public void ApplyPosition(VisualElement popup)
        {
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;
            popup.style.left = mousePos.x + Offset.x;
            popup.style.top = mousePos.y + Offset.y;
        }

        public void OnDestroyed(VisualElement popup, VisualElement root) { }
    }

    public class CenterScreenStrategy : IPopupPositionStrategy
    {
        public Vector2 Offset = new Vector2(0, 0);

        public void OnCreated(VisualElement popup, VisualElement root)
        {
            popup.style.position = Position.Absolute;
            root.Add(popup);
        }

        public void ApplyPosition(VisualElement popup)
        {
            popup.style.left = (Screen.width / 2f) + Offset.x;
            popup.style.top = (Screen.height / 2f) + Offset.y;
            popup.style.translate = new StyleTranslate(new Translate(Length.Percent(-50), Length.Percent(-50), 0));
        }

        public void OnDestroyed(VisualElement popup, VisualElement root) { }
    }
}