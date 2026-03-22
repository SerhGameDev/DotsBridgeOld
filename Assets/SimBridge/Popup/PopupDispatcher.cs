using System;
using UnityEngine;
using DotsBridge;
using SimElectric; // Твой неймспейс для ElectricalNode

namespace DotsBridge.UI
{
    /// <summary>
    /// Мост между событиями DOTS (OnMouseEnter) и UI Toolkit.
    /// </summary>
    public class PopupDispatcher : MonoBehaviour
    {
        // События, на которые потом подпишется менеджер UI Toolkit
        public static event Action<PopupBuilder> OnShowPopup;
        public static event Action OnHidePopup;

        private PopupBuilder _builder;

        private void Awake()
        {
            _builder = new PopupBuilder();
        }

        private void OnEnable()
        {
            EntityBridge.OnMouseEnter += HandleMouseEnter;
            EntityBridge.OnMouseExit += HandleMouseExit;
        }

        private void OnDisable()
        {
            EntityBridge.OnMouseEnter -= HandleMouseEnter;
            EntityBridge.OnMouseExit -= HandleMouseExit;
        }

        private void HandleMouseEnter(SingleEntity entity)
        {
            if (entity.Entity == Unity.Entities.Entity.Null) return;

            _builder.Clear();
            bool hasData = false;

            // 1. Проверяем электрический узел
            if (entity.HasComponent<ElectricalNode>())
            {
                var node = entity.GetComponent<ElectricalNode>();
                _builder.AddHeader("Узел цепи")
                        .AddValue("Напряжение", node.CurrentVoltage, "V")
                        .AddValue("Сопротивление", node.Resistance, "Ω")
                        .AddStatus("Источник питания", node.IsPowerSource)
                        .AddStatus("Заземлен", node.IsGrounded);
                hasData = true;
            }

            // Если нашли хоть какие-то данные, отправляем сигнал на отрисовку
            if (hasData)
            {
                OnShowPopup?.Invoke(_builder);
            }
        }

        private void HandleMouseExit(SingleEntity entity)
        {
            // Говорим UI спрятать окно
            OnHidePopup?.Invoke();
        }
    }
}