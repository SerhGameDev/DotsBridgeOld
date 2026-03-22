using System;
using System.Collections.Generic;
using System.Linq;
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

        private VisualElement _root;
        private readonly List<PopupController> _registeredPopups = new List<PopupController>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            if (Document != null) _root = Document.rootVisualElement;
            
            // Подписываемся напрямую на события моста
            EntityBridge.OnMouseEnter += HandleMouseEnter;
            EntityBridge.OnMouseExit += HandleMouseExit;
        }

        private void OnDisable()
        {
            EntityBridge.OnMouseEnter -= HandleMouseEnter;
            EntityBridge.OnMouseExit -= HandleMouseExit;
        }

        private void Update()
        {
            // Обновляем позиции всех открытых окон
            foreach (var popup in _registeredPopups)
            {
                popup.UpdatePosition();
            }
        }
        public void RegisterDefinition(IPopupDefinition definition)
        {
            var controller = new PopupController(this, definition);
            _registeredPopups.Add(controller);
        }
        /// <summary>
        /// Главный метод для регистрации новых окон из любого места в коде.
        /// </summary>
        public PopupController RegisterPopup(Func<SingleEntity, bool> condition)
        {
            var controller = new PopupController(this, condition);
            _registeredPopups.Add(controller);
            return controller;
        }

        // --- Обработчики событий ---

        private void HandleMouseEnter(SingleEntity entity)
        {
            if (_root == null) return;
            // Каждое зарегистрированное окно само проверит, нужно ли ему открываться
            foreach (var popup in _registeredPopups)
            {
                popup.TryOpen(entity, _root);
            }
        }

        private void HandleMouseExit(SingleEntity entity)
        {
            foreach (var popup in _registeredPopups)
            {
                popup.Close(_root);
            }
        }

        // --- Фабричные методы для Controller'ов ---

        internal VisualElement InstantiateBasePopup()
        {
            return PopupBaseTemplate.Instantiate().Children().First();
        }

        internal VisualElement CreateRowElement(PopupRowData row)
        {
            VisualElement rowElement = null;
            switch (row.Type)
            {
                case PopupRowType.Header:
                    if (HeaderTemplate) { rowElement = HeaderTemplate.Instantiate(); rowElement.Q<Label>("HeaderLabel").text = row.Label; }
                    break;
                case PopupRowType.Text:
                    if (TextTemplate) { rowElement = TextTemplate.Instantiate(); rowElement.Q<Label>("NameLabel").text = row.Label; rowElement.Q<Label>("ValueLabel").text = row.StringValue; }
                    break;
                case PopupRowType.Value:
                    if (ValueTemplate) { rowElement = ValueTemplate.Instantiate(); rowElement.Q<Label>("NameLabel").text = row.Label; rowElement.Q<Label>("ValueLabel").text = $"{row.FloatValue} {row.StringValue}"; }
                    break;
                case PopupRowType.Status:
                    if (StatusTemplate) { 
                        rowElement = StatusTemplate.Instantiate(); 
                        rowElement.Q<Label>("NameLabel").text = row.Label; 
                        var lbl = rowElement.Q<Label>("StatusLabel");
                        lbl.text = row.BoolValue ? "ON" : "OFF";
                        lbl.style.color = row.BoolValue ? new StyleColor(Color.green) : new StyleColor(Color.red);
                    }
                    break;
            }
            return rowElement;
        }
    }
}