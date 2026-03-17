using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public class HierarchyDragManipulator : PointerManipulator
    {
        private bool _isDragging;
        private Vector2 _startPosition; // Теперь храним в мировых координатах
        
        private readonly Action<HierarchyDragManipulator, Vector2> _onDragStart;
        private readonly Action<HierarchyDragManipulator, Vector2> _onDragUpdate; 
        private readonly Action<HierarchyDragManipulator, Vector2> _onDragEnd;

        public HierarchyViewElement Element { get; }

        public HierarchyDragManipulator(HierarchyViewElement element, 
            Action<HierarchyDragManipulator, Vector2> onDragStart, 
            Action<HierarchyDragManipulator, Vector2> onDragUpdate,
            Action<HierarchyDragManipulator, Vector2> onDragEnd)
        {
            Element = element;
            _onDragStart = onDragStart;
            _onDragUpdate = onDragUpdate;
            _onDragEnd = onDragEnd;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut, TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut, TrickleDown.TrickleDown);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            
            _isDragging = false;
            // Переводим локальный клик в координаты панели
            _startPosition = target.LocalToWorld(evt.localPosition);
            
            target.CapturePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!target.HasPointerCapture(evt.pointerId)) return;

            // Текущая позиция в координатах панели
            Vector2 currentPanelPos = target.LocalToWorld(evt.localPosition);

            if (!_isDragging && Vector2.Distance(_startPosition, currentPanelPos) > 10f)
            {
                _isDragging = true;
                _onDragStart?.Invoke(this, currentPanelPos);
            }

            if (_isDragging)
            {
                _onDragUpdate?.Invoke(this, currentPanelPos);
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!target.HasPointerCapture(evt.pointerId)) return;
            target.ReleasePointer(evt.pointerId);

            if (_isDragging)
            {
                _isDragging = false;
                Vector2 finalPanelPos = target.LocalToWorld(evt.localPosition);
                _onDragEnd?.Invoke(this, finalPanelPos);
            }
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _onDragEnd?.Invoke(this, Vector2.zero);
            }
        }
    }
}