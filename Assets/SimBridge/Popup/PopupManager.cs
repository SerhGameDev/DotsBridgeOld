using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace DotsBridge.UI
{
    /// <summary>
    /// Глобальный менеджер всплывающих окон. Синглтон.
    /// Отвечает за рендеринг данных из PopupBuilder в UI Toolkit.
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        public static PopupManager Instance { get; private set; }

        [Header("UI Document")]
        public UIDocument Document;

        [Header("UXML Templates")]
        [Tooltip("Базовое окно. Должно содержать VisualElement с именем 'PopupContent'")]
        public VisualTreeAsset PopupBaseTemplate;
        
        [Tooltip("Шаблон заголовка. Ожидает Label с именем 'HeaderLabel'")]
        public VisualTreeAsset HeaderTemplate;
        
        [Tooltip("Шаблон текста. Ожидает Label 'NameLabel' и 'ValueLabel'")]
        public VisualTreeAsset TextTemplate;
        
        [Tooltip("Шаблон числа. Ожидает Label 'NameLabel' и 'ValueLabel'")]
        public VisualTreeAsset ValueTemplate;
        
        [Tooltip("Шаблон статуса (bool). Ожидает Label 'NameLabel' и 'StatusLabel'")]
        public VisualTreeAsset StatusTemplate;

        [Header("Settings")]
        public Vector2 CursorOffset = new Vector2(15, 15);

        private VisualElement _root;
        private VisualElement _popupInstance;
        private VisualElement _contentContainer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (Document != null) _root = Document.rootVisualElement;
            
            // Подписываемся на диспетчер
            PopupDispatcher.OnShowPopup += BuildAndShowPopup;
            PopupDispatcher.OnHidePopup += HidePopup;
        }

        private void OnDisable()
        {
            PopupDispatcher.OnShowPopup -= BuildAndShowPopup;
            PopupDispatcher.OnHidePopup -= HidePopup;
        }

        private void BuildAndShowPopup(PopupBuilder builder)
        {
            if (_root == null || PopupBaseTemplate == null) return;

            // Очищаем предыдущее окно, если мышка резко перескочила с объекта на объект
            HidePopup();

            // Создаем основу окна
            // Instantiate() возвращает TemplateContainer, нам нужен его первый дочерний элемент
            _popupInstance = PopupBaseTemplate.Instantiate().Children().First();
            _popupInstance.style.position = Position.Absolute;
            
            // Ищем контейнер для строк. Если не нашли по имени, используем сам корень попапа
            _contentContainer = _popupInstance.Q<VisualElement>("PopupContent") ?? _popupInstance;

            // Динамически собираем строки на основе данных
            foreach (var row in builder.Rows)
            {
                VisualElement rowElement = null;

                switch (row.Type)
                {
                    case PopupRowType.Header:
                        if (HeaderTemplate != null)
                        {
                            rowElement = HeaderTemplate.Instantiate();
                            rowElement.Q<Label>("HeaderLabel").text = row.Label;
                        }
                        break;

                    case PopupRowType.Text:
                        if (TextTemplate != null)
                        {
                            rowElement = TextTemplate.Instantiate();
                            rowElement.Q<Label>("NameLabel").text = row.Label;
                            rowElement.Q<Label>("ValueLabel").text = row.StringValue;
                        }
                        break;

                    case PopupRowType.Value:
                        if (ValueTemplate != null)
                        {
                            rowElement = ValueTemplate.Instantiate();
                            rowElement.Q<Label>("NameLabel").text = row.Label;
                            // Склеиваем значение и единицу измерения (например, "230 V")
                            rowElement.Q<Label>("ValueLabel").text = $"{row.FloatValue} {row.StringValue}";
                        }
                        break;

                    case PopupRowType.Status:
                        if (StatusTemplate != null)
                        {
                            rowElement = StatusTemplate.Instantiate();
                            rowElement.Q<Label>("NameLabel").text = row.Label;
                            var statusLabel = rowElement.Q<Label>("StatusLabel");
                            
                            statusLabel.text = row.BoolValue ? "ON" : "OFF";
                            // Пример стилизации из кода (можно перенести в USS классы)
                            statusLabel.style.color = row.BoolValue ? new StyleColor(Color.green) : new StyleColor(Color.red);
                        }
                        break;
                }

                if (rowElement != null)
                {
                    _contentContainer.Add(rowElement);
                }
            }

            _root.Add(_popupInstance);
            UpdatePopupPosition(); // Вызываем сразу, чтобы окно не мелькнуло в углу экрана
        }

        private void HidePopup()
        {
            if (_popupInstance != null && _root.Contains(_popupInstance))
            {
                _root.Remove(_popupInstance);
                _popupInstance = null;
            }
        }

        private void Update()
        {
            if (_popupInstance != null)
            {
                UpdatePopupPosition();
            }
        }

        private void UpdatePopupPosition()
        {
            if (_popupInstance == null) return;

            Vector2 mousePos = Input.mousePosition;
            
            // ВАЖНО: Координаты мыши в Unity начинаются снизу-слева (0,0).
            // А в UI Toolkit (и вебе) координаты начинаются сверху-слева (0,0).
            // Поэтому инвертируем ось Y.
            mousePos.y = Screen.height - mousePos.y;

            _popupInstance.style.left = mousePos.x + CursorOffset.x;
            _popupInstance.style.top = mousePos.y + CursorOffset.y;
        }
    }
}