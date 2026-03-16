using UnityEngine;
using UnityEngine.UIElements;

public class WorkspaceManager
{
    public VisualElement ContentContainer { get; private set; } // Сюда теперь будем добавлять ноды!
    
    private VisualElement viewport;
    
    // Внутренние переменные состояния
    private Vector2 panOffset = Vector2.zero;
    private float currentZoom = 1f;
    private bool isPanning = false;
    private Vector2 panStartMousePos;
    private Vector2 panStartOffset;

    // Настройки (будут передаваться из EditorContext)
    private float minZoom;
    private float maxZoom;
    private float zoomSpeed;

    public WorkspaceManager(VisualElement viewport, float minZoom, float maxZoom, float zoomSpeed)
    {
        this.viewport = viewport;
        this.minZoom = minZoom;
        this.maxZoom = maxZoom;
        this.zoomSpeed = zoomSpeed;

        // Создаем контейнер для контента программно, чтобы не переделывать UXML
        ContentContainer = new VisualElement { name = "content-container" };
        ContentContainer.style.position = Position.Absolute;
        
        // ВАЖНО: Точка трансформации в левый верхний угол для правильной математики зума
        ContentContainer.style.transformOrigin = new TransformOrigin(0, 0); 
        
        viewport.Add(ContentContainer);

        // Подписываемся на события мыши (Колесико и средняя кнопка)
        viewport.RegisterCallback<WheelEvent>(OnWheel);
        viewport.RegisterCallback<PointerDownEvent>(OnPointerDown);
        viewport.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        viewport.RegisterCallback<PointerUpEvent>(OnPointerUp);
        viewport.RegisterCallback<PointerCaptureOutEvent>(evt => isPanning = false);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        // Кнопка 2 - это колесико мыши (Middle Click)
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
        // Позиция мыши относительно вьюпорта
        Vector2 mousePos = evt.localMousePosition; 
        
        float oldZoom = currentZoom;
        
        // evt.delta.y > 0 это скролл вниз (отдаление), < 0 скролл вверх (приближение)
        float zoomDelta = -evt.delta.y * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom + zoomDelta, minZoom, maxZoom);

        // Математика зума к курсору
        float ratio = currentZoom / oldZoom;
        panOffset = mousePos - (mousePos - panOffset) * ratio;

        ApplyTransform();
        evt.StopPropagation();
    }

    private void ApplyTransform()
    {
        // Применяем позицию и масштаб к контейнеру
        ContentContainer.transform.position = panOffset;
        ContentContainer.transform.scale = new Vector3(currentZoom, currentZoom, 1f);
    }
}