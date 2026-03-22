using UnityEngine;
using UnityEngine.UIElements;

namespace DotsBridge.UI.Positioning
{
    /// <summary>
    /// Базовый класс для всех стратегий позиционирования всплывающих окон.
    /// </summary>
    public abstract class PopupPositionStrategy : MonoBehaviour
    {
        /// <summary>
        /// Вызывается единожды при сборке окна. 
        /// Полезно для встраивания элемента в существующий Flexbox-контейнер.
        /// </summary>
        public virtual void OnPopupCreated(VisualElement popup, VisualElement root) { }

        /// <summary>
        /// Вызывается каждый кадр (или при движении мыши) для обновления координат.
        /// </summary>
        public abstract void ApplyPosition(VisualElement popup);

        /// <summary>
        /// Вызывается перед уничтожением окна.
        /// </summary>
        public virtual void OnPopupDestroyed(VisualElement popup, VisualElement root) { }
    }
    public class MouseCursorStrategy : PopupPositionStrategy
    {
        public Vector2 Offset = new Vector2(15, 15);

        public override void OnPopupCreated(VisualElement popup, VisualElement root)
        {
            popup.style.position = Position.Absolute;
            root.Add(popup); // Добавляем в корень экрана
        }

        public override void ApplyPosition(VisualElement popup)
        {
            if (popup == null) return;

            Vector2 mousePos = Input.mousePosition;
            // Инверсия Y для UI Toolkit
            mousePos.y = Screen.height - mousePos.y;

            popup.style.left = mousePos.x + Offset.x;
            popup.style.top = mousePos.y + Offset.y;
        }
    }
    public class CenterScreenStrategy : PopupPositionStrategy
    {
        public Vector2 Offset = new Vector2(20, -20);

        public override void OnPopupCreated(VisualElement popup, VisualElement root)
        {
            popup.style.position = Position.Absolute;
            root.Add(popup);
        }

        public override void ApplyPosition(VisualElement popup)
        {
            if (popup == null) return;

            // Ставим ровно по центру
            popup.style.left = (Screen.width / 2f) + Offset.x;
            popup.style.top = (Screen.height / 2f) + Offset.y;
            
            // Сдвигаем сам элемент на 50% его размера, чтобы центр окна совпадал с центром экрана
            popup.style.translate = new StyleTranslate(new Translate(Length.Percent(-50), Length.Percent(-50), 0));
        }
    }
    public class EmbeddedUIStrategy : PopupPositionStrategy
    {
        [Tooltip("Имя VisualElement (контейнера) в UIDocument, куда встроится окно")]
        public string TargetContainerName = "SideInfoPanel";

        public override void OnPopupCreated(VisualElement popup, VisualElement root)
        {
            var container = root.Q<VisualElement>(TargetContainerName);
            if (container != null)
            {
                // Сбрасываем абсолютное позиционирование.
                // Теперь окно подчиняется правилам Flexbox целевого контейнера.
                popup.style.position = Position.Relative;
                popup.style.left = StyleKeyword.Null;
                popup.style.top = StyleKeyword.Null;
                popup.style.translate = StyleKeyword.Null;

                container.Add(popup);
            }
            else
            {
                Debug.LogWarning($"[DotsBridge] Контейнер '{TargetContainerName}' не найден!");
            }
        }

        public override void ApplyPosition(VisualElement popup)
        {
            // Ничего не делаем в Update. Встроенный UI статичен и управляется Flexbox'ом.
        }
    }
}