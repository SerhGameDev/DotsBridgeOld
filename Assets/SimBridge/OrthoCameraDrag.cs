using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrthoCameraController : MonoBehaviour
{
    [Header("Перемещение (Panning)")]
    [SerializeField] private MouseButton panButton = MouseButton.Right;
    [SerializeField] private float panSmoothing = 10f;

    [Header("Масштабирование (Zoom)")]
    [SerializeField] private float zoomSensitivity = 2f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private float zoomSmoothing = 10f;

    private Camera _cam;
    private Vector3 _targetPosition;
    private float _targetZoom;
    private Vector3 _dragOrigin;

    public enum MouseButton { Left = 0, Right = 1, Middle = 2 }

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _targetPosition = transform.position;
        _targetZoom = _cam.orthographicSize;
    }

    private void LateUpdate()
    {
        HandlePan();
        HandleZoom();

        // Применяем плавное перемещение (Interpolation)
        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * panSmoothing);
        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetZoom, Time.deltaTime * zoomSmoothing);
    }

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown((int)panButton))
        {
            _dragOrigin = _cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton((int)panButton))
        {
            Vector3 currentPos = _cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = _dragOrigin - currentPos;

            // Важно: не меняем Z, чтобы не "пролететь" сквозь объекты
            _targetPosition = new Vector3(
                _targetPosition.x + difference.x,
                _targetPosition.y + difference.y,
                _targetPosition.z
            );
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            _targetZoom -= scroll * zoomSensitivity * (_targetZoom / 2); // Коэффициент делает зум плавнее на малых величинах
            _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);
        }
    }
}