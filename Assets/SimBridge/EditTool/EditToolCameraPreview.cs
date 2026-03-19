using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EditTool
{
    [RequireComponent(typeof(Camera))]
    public class EditToolCameraPreview : MonoBehaviour
    {
        private Camera _cam;
        private Camera TargetCamera => _cam != null ? _cam : (_cam = GetComponent<Camera>());

        [TitleGroup("Настройка фокуса")]
        [Tooltip("Точка, вокруг которой будет вращаться камера. Если не указана, облет работать не будет.")]
        public Transform focusTarget;

        // ==========================================
        // УПРАВЛЕНИЕ И СИНХРОНИЗАЦИЯ
        // ==========================================

        [TitleGroup("Управление Scene View")]
        [Button("👀 Установить вид из Камеры", ButtonSizes.Large)]
        public void SyncSceneView()
        {
#if UNITY_EDITOR
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) return;

            view.orthographic = TargetCamera.orthographic;
            view.AlignViewToObject(TargetCamera.transform);

            if (TargetCamera.orthographic)
            {
                view.size = TargetCamera.orthographicSize * 2f;
                previewZoom = TargetCamera.orthographicSize; // Обновляем ползунок зума
            }

            if (focusTarget != null)
            {
                view.pivot = focusTarget.position;
            }

            view.Repaint();
#endif
        }

        [TitleGroup("Управление Scene View")]
        [Button("📸 Настроить Камеру по Scene View", ButtonSizes.Large)]
        public void SyncCameraToScene()
        {
#if UNITY_EDITOR
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) return;

            // Регистрируем изменения для отмены (Ctrl+Z)
            Undo.RecordObject(TargetCamera.transform, "Sync Camera Transform");
            Undo.RecordObject(TargetCamera, "Sync Camera Properties");

            // Переносим позицию и поворот из окна сцены в игровую камеру
            TargetCamera.transform.position = view.camera.transform.position;
            TargetCamera.transform.rotation = view.camera.transform.rotation;
            
            // Если камера ортографическая, переносим зум
            if (TargetCamera.orthographic)
            {
                TargetCamera.orthographicSize = view.size / 2f;
                previewZoom = TargetCamera.orthographicSize; // Обновляем ползунок зума
            }

            Debug.Log($"[EditTool] Игровая камера '{gameObject.name}' настроена по виду из Scene View.");
#endif
        }

        // ==========================================
        // ВРАЩЕНИЕ ВОКРУГ ТОЧКИ (ORBIT)
        // ==========================================

        [TitleGroup("Вращение вокруг фокуса (Ортогонально)")]
        [InfoBox("Для работы вращения необходимо назначить Focus Target.", InfoMessageType.Warning, "@focusTarget == null")]
        
        [Range(0f, 360f), OnValueChanged("OrbitCamera"), EnableIf("@focusTarget != null")]
        [LabelText("Облет (Горизонталь)")]
        public float orbitAngle = 45f;

        [TitleGroup("Вращение вокруг фокуса (Ортогонально)")]
        [Range(-90f, 90f), OnValueChanged("OrbitCamera"), EnableIf("@focusTarget != null")]
        [LabelText("Высота (Вертикаль)")]
        public float elevationAngle = 30f;

        [TitleGroup("Вращение вокруг фокуса (Ортогонально)")]
        [MinValue(0.1f), OnValueChanged("OrbitCamera"), EnableIf("@focusTarget != null")]
        [LabelText("Приближение (Zoom)")]
        public float previewZoom = 5f;

        private void OrbitCamera()
        {
#if UNITY_EDITOR
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null || focusTarget == null) return;

            view.orthographic = true;
            view.pivot = focusTarget.position;
            view.rotation = Quaternion.Euler(elevationAngle, orbitAngle, 0f);
            
            // Используем настраиваемое приближение
            view.size = previewZoom * 2f;

            view.Repaint();
#endif
        }

        // ==========================================
        // ИЗОМЕТРИЧЕСКИЕ РАКУРСЫ
        // ==========================================

        [TitleGroup("Изометрические ракурсы")]
        [InfoBox("Быстрое переключение между четырьмя диагональными изометрическими видами.")]
        
        [HorizontalGroup("Изометрические ракурсы/Row1")]
        [Button("Изо: Спереди-Слева", ButtonSizes.Medium), EnableIf("@focusTarget != null")]
        public void ViewIsoFrontLeft() { orbitAngle = 315f; elevationAngle = 30f; OrbitCamera(); }

        [HorizontalGroup("Изометрические ракурсы/Row1")]
        [Button("Изо: Спереди-Справа", ButtonSizes.Medium), EnableIf("@focusTarget != null")]
        public void ViewIsoFrontRight() { orbitAngle = 45f; elevationAngle = 30f; OrbitCamera(); }

        [HorizontalGroup("Изометрические ракурсы/Row2")]
        [Button("Изо: Сзади-Слева", ButtonSizes.Medium), EnableIf("@focusTarget != null")]
        public void ViewIsoBackLeft() { orbitAngle = 225f; elevationAngle = 30f; OrbitCamera(); }

        [HorizontalGroup("Изометрические ракурсы/Row2")]
        [Button("Изо: Сзади-Справа", ButtonSizes.Medium), EnableIf("@focusTarget != null")]
        public void ViewIsoBackRight() { orbitAngle = 135f; elevationAngle = 30f; OrbitCamera(); }
    }
}