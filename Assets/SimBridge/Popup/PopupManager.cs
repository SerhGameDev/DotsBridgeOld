using System.Linq;
using DotsBridge.UI.Positioning; // Не забудь добавить namespace
using UnityEngine;
using UnityEngine.UIElements;

namespace DotsBridge.UI
{
    public class PopupManager : MonoBehaviour
    {
        public static PopupManager Instance { get; private set; }

        [Header("UI Document & Templates")]
        public UIDocument Document;
        public VisualTreeAsset PopupBaseTemplate;
        public VisualTreeAsset HeaderTemplate;
        public VisualTreeAsset TextTemplate;
        public VisualTreeAsset ValueTemplate;
        public VisualTreeAsset StatusTemplate;

        [Header("Positioning")]
        [Tooltip("Перетащи сюда компонент стратегии (например, MouseCursorStrategy)")]
        public PopupPositionStrategy PositionStrategy;

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
            
            // Если стратегия не назначена вручную, ищем её на этом же GameObject
            if (PositionStrategy == null)
            {
                PositionStrategy = GetComponent<PopupPositionStrategy>();
            }
        }

        private void OnEnable()
        {
            if (Document != null) _root = Document.rootVisualElement;
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
            if (_root == null || PopupBaseTemplate == null || PositionStrategy == null) return;

            HidePopup();

            _popupInstance = PopupBaseTemplate.Instantiate().Children().First();
            _contentContainer = _popupInstance.Q<VisualElement>("PopupContent") ?? _popupInstance;

            // ... (Код генерации строк остается без изменений из предыдущего шага) ...
            // foreach (var row in builder.Rows) { ... }

            // Делегируем логику добавления и начальной настройки стратегии
            PositionStrategy.OnPopupCreated(_popupInstance, _root);
            
            // Сразу применяем координаты, чтобы избежать фликера в 1 кадр
            PositionStrategy.ApplyPosition(_popupInstance); 
        }

        private void HidePopup()
        {
            if (_popupInstance != null)
            {
                if (PositionStrategy != null)
                {
                    PositionStrategy.OnPopupDestroyed(_popupInstance, _root);
                }

                if (_popupInstance.parent != null)
                {
                    _popupInstance.parent.Remove(_popupInstance);
                }
                _popupInstance = null;
            }
        }

        private void Update()
        {
            if (_popupInstance != null && PositionStrategy != null)
            {
                // Стратегия сама решает, нужно ли двигать элемент
                PositionStrategy.ApplyPosition(_popupInstance);
            }
        }
    }
}