#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace EditTool
{
    [InitializeOnLoad]
    public static class EditToolSnapper
    {
        private static float snapRadius = 1; // На каком расстоянии срабатывает магнит
        private static int lastHotControl = 0;
        
        // Кэширование для производительности при работе со сложными префабами
        private static GameObject lastSelectedObject;
        private static EditToolSocket[] cachedMovingSockets;

        static EditToolSnapper()
        {
            // Подписываемся на обновление окна сцены
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (Application.isPlaying) return;
            if (Selection.gameObjects.Length != 1) return;

            GameObject activeObj = Selection.activeGameObject;

            // Обновляем кэш сокетов только при смене выделенного объекта (спасает FPS)
            if (activeObj != lastSelectedObject)
            {
                lastSelectedObject = activeObj;
                cachedMovingSockets = activeObj.GetComponentsInChildren<EditToolSocket>();
            }

            if (cachedMovingSockets == null || cachedMovingSockets.Length == 0) return;

            // Unity отслеживает взаимодействие с манипуляторами через GUIUtility.hotControl
            int currentHotControl = GUIUtility.hotControl;
            bool isDragging = currentHotControl != 0;
            bool justDropped = (lastHotControl != 0 && currentHotControl == 0); // Событие "только что отпустили мышь"
            lastHotControl = currentHotControl;

            if (!isDragging && !justDropped) return;

            EditToolSocket bestMoving = null;
            EditToolSocket bestTarget = null;
            float minDistance = float.MaxValue;

            // Ищем ближайшую пару совместимых сокетов
            foreach (var moving in cachedMovingSockets)
            {
                if (moving.IsConnected) continue;

                foreach (var target in EditToolSocket.AllSockets)
                {
                    // Пропускаем занятые сокеты, сокеты на том же объекте и несовместимые по типу
                    if (target.IsConnected || cachedMovingSockets.Contains(target) || !moving.CanConnectTo(target)) continue;

                    float dist = Vector3.Distance(moving.transform.position, target.transform.position);
                    if (dist < minDistance && dist <= snapRadius)
                    {
                        minDistance = dist;
                        bestMoving = moving;
                        bestTarget = target;
                    }
                }
            }

            // Если нашли пару в радиусе притяжения
            if (bestMoving != null && bestTarget != null)
            {
                // Рисуем красивую линию превью
                Handles.color = Color.cyan;
                Handles.DrawDottedLine(bestMoving.transform.position, bestTarget.transform.position, 4f);

                // Если пользователь отпустил объект — выполняем автостыковку
                if (justDropped)
                {
                    Transform rootToMove = bestMoving.rootTransform != null ? bestMoving.rootTransform : activeObj.transform;

                    Undo.RecordObject(rootToMove, "Auto Snap Position");

                    // Применяем ту же математику выравнивания
                    Quaternion desiredRot = Quaternion.LookRotation(-bestTarget.Direction, bestTarget.transform.up);
                    Quaternion rotDelta = desiredRot * Quaternion.Inverse(bestMoving.transform.rotation);
                    rootToMove.rotation = rotDelta * rootToMove.rotation;

                    Vector3 posDelta = bestTarget.transform.position - bestMoving.transform.position;
                    rootToMove.position += posDelta;

                    // Регистрируем соединения
                    Undo.RecordObject(bestMoving, "Auto Connect");
                    Undo.RecordObject(bestTarget, "Auto Connect");
                    bestMoving.Connect(bestTarget);

                    Debug.Log($"[EditTool] Автоматическая стыковка: {rootToMove.name}");
                }
                
                // Заставляем окно сцены обновляться для плавной отрисовки линии
                sceneView.Repaint();
            }
        }
    }
}
#endif