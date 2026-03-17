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
        private readonly Action<HierarchyDragManipulator, Vector2> _onDragEnd;

        public HierarchyViewElement Element { get; }

        public HierarchyDragManipulator(HierarchyViewElement element, 
            Action<HierarchyDragManipulator, Vector2> onDragStart, 
            Action<HierarchyDragManipulator, Vector2> onDragEnd)
        {
            Element = element;
            _onDragStart = onDragStart;
            _onDragEnd = onDragEnd;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return; // Только левая кнопка мыши

            _isDragging = false;
            _startPosition = evt.position;
            target.CapturePointer(evt.pointerId); // Захватываем мышь, чтобы события шли только сюда
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!target.HasPointerCapture(evt.pointerId)) return;

            // Если сдвинули мышь больше чем на 5 пикселей — начинаем тащить
            if (!_isDragging && Vector2.Distance(_startPosition, evt.position) > 5f)
            {
                _isDragging = true;
                _onDragStart?.Invoke(this, evt.position);
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

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _onDragEnd?.Invoke(this, Vector2.zero); // Отмена
            }
        }
    }
}