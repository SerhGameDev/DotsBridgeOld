using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    public class HierarchyDragManipulator : PointerManipulator
    {
        private bool _isDragging;
        private Vector2 _startPosition;
        
        private readonly Action<HierarchyDragManipulator, Vector2> _onDragStart;
        private readonly Action<HierarchyDragManipulator, Vector2> _onDragUpdate; // Новое событие
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
        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _onDragEnd?.Invoke(this, Vector2.zero); 
            }
        }
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            _isDragging = false;
            _startPosition = evt.position;
            target.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!target.HasPointerCapture(evt.pointerId)) return;

            if (!_isDragging && Vector2.Distance(_startPosition, evt.position) > 10f)
            {
                _isDragging = true;
                _onDragStart?.Invoke(this, evt.position);
            }

            if (_isDragging)
            {
                _onDragUpdate?.Invoke(this, evt.position);
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!target.HasPointerCapture(evt.pointerId)) return;
            target.ReleasePointer(evt.pointerId);

            if (_isDragging)
            {
                _isDragging = false;
                _onDragEnd?.Invoke(this, evt.position);
            }
        }
    }
}