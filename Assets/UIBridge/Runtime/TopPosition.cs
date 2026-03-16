using UnityEngine;

namespace UIBridge
{
    public class TopPosition : ViewPosition
    {
        public override void ApplyTo(ViewContainer container)
        {
            // Здесь логика RectTransform: установка anchors в верхнюю часть экрана
            // container.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1); и т.д.
            Debug.Log($"Applying Top Position to {container.name}");
        }
    }
}