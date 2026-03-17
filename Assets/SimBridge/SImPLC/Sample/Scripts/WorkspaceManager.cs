using UnityEngine;
using UnityEngine.UIElements;

namespace SimPLS
{
    public class WorkspaceManager
    {
        public VisualElement ContentContainer { get; private set; }
        
        public float Zoom => currentZoom; 

        private VisualElement viewport;
        private Vector2 panOffset = Vector2.zero;
        private float currentZoom = 1f;
        private bool isPanning = false;
        private Vector2 panStartMousePos;
        private Vector2 panStartOffset;

        private float minZoom;
        private float maxZoom;
        private float zoomSpeed;

        public WorkspaceManager(VisualElement viewport, float minZoom, float maxZoom, float zoomSpeed)
        {
            this.viewport = viewport;
            this.minZoom = minZoom;
            this.maxZoom = maxZoom;
            this.zoomSpeed = zoomSpeed;

            ContentContainer = new VisualElement { name = "content-container" };
            ContentContainer.style.position = Position.Absolute;
            ContentContainer.style.transformOrigin = new TransformOrigin(0, 0); 
            
            viewport.Add(ContentContainer);

            viewport.RegisterCallback<WheelEvent>(OnWheel);
            viewport.RegisterCallback<PointerDownEvent>(OnPointerDown);
            viewport.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            viewport.RegisterCallback<PointerUpEvent>(OnPointerUp);
            viewport.RegisterCallback<PointerCaptureOutEvent>(evt => isPanning = false);
        }

        // --- НОВОЕ: Метод правильной конвертации координат ---
        public Vector2 ScreenToWorkspace(Vector2 screenPosition)
        {
            return (screenPosition - panOffset) / currentZoom;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 2)
            {
                isPanning = true;
                panStartMousePos = evt.position;
                panStartOffset = panOffset;
                viewport.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!isPanning || !viewport.HasPointerCapture(evt.pointerId)) return;
            Vector2 delta = evt.position - (Vector3)panStartMousePos;
            panOffset = panStartOffset + delta;
            ApplyTransform();
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (isPanning && viewport.HasPointerCapture(evt.pointerId))
            {
                isPanning = false;
                viewport.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            }
        }

        private void OnWheel(WheelEvent evt)
        {
            Vector2 mousePos = evt.localMousePosition; 
            float oldZoom = currentZoom;
            
            float zoomDelta = -evt.delta.y * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom + zoomDelta, minZoom, maxZoom);

            float ratio = currentZoom / oldZoom;
            panOffset = mousePos - (mousePos - panOffset) * ratio;

            ApplyTransform();
            evt.StopPropagation();
        }

        private void ApplyTransform()
        {
            ContentContainer.transform.position = panOffset;
            ContentContainer.transform.scale = new Vector3(currentZoom, currentZoom, 1f);
        }
    }
}