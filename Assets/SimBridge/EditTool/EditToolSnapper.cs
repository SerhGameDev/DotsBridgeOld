#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace EditTool
{
    [InitializeOnLoad]
    public static class EditToolSnapper
    {
        private static float snapRadius = 1.5f; 
        private static int lastHotControl = 0;
        
        private static GameObject lastSelectedObject;
        private static EditToolSocket[] cachedMovingSockets;
        private static EditToolSocket[] allSocketsInScene; // Кэш всех сокетов на сцене

        static EditToolSnapper()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (Application.isPlaying) return;
            if (Selection.gameObjects.Length != 1) return;

            GameObject activeObj = Selection.activeGameObject;

            if (activeObj != lastSelectedObject)
            {
                lastSelectedObject = activeObj;
                cachedMovingSockets = activeObj.GetComponentsInChildren<EditToolSocket>();
            }

            if (cachedMovingSockets == null || cachedMovingSockets.Length == 0) return;

            int currentHotControl = GUIUtility.hotControl;
            bool isDragging = currentHotControl != 0;
            bool justStartedDragging = (lastHotControl == 0 && currentHotControl != 0); // Только начали тащить
            bool justDropped = (lastHotControl != 0 && currentHotControl == 0); 
            lastHotControl = currentHotControl;

            // Как только начали тащить объект - находим ВСЕ сокеты на сцене один раз
            if (justStartedDragging)
            {
                allSocketsInScene = Object.FindObjectsOfType<EditToolSocket>();
            }

            if (!isDragging && !justDropped) return;
            if (allSocketsInScene == null) return;

            EditToolSocket bestMoving = null;
            EditToolSocket bestTarget = null;
            float minDistance = float.MaxValue;

            foreach (var moving in cachedMovingSockets)
            {
                if (moving.IsConnected) continue;

                foreach (var target in allSocketsInScene)
                {
                    // Защита от удаленных объектов (если удалили во время перетаскивания)
                    if (target == null) continue;

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

            if (bestMoving != null && bestTarget != null)
            {
                Handles.color = Color.cyan;
                Handles.DrawDottedLine(bestMoving.transform.position, bestTarget.transform.position, 4f);

                if (justDropped)
                {
                    Transform rootToMove = bestMoving.rootTransform != null ? bestMoving.rootTransform : activeObj.transform;

                    Undo.RecordObject(rootToMove, "Auto Snap Position");

                    Quaternion desiredRot = Quaternion.LookRotation(-bestTarget.Direction, bestTarget.transform.up);
                    Quaternion rotDelta = desiredRot * Quaternion.Inverse(bestMoving.transform.rotation);
                    rootToMove.rotation = rotDelta * rootToMove.rotation;

                    Vector3 posDelta = bestTarget.transform.position - bestMoving.transform.position;
                    rootToMove.position += posDelta;

                    Undo.RecordObject(bestMoving, "Auto Connect");
                    Undo.RecordObject(bestTarget, "Auto Connect");
                    bestMoving.Connect(bestTarget);

                    Debug.Log($"[EditTool] Автоматическая стыковка: {rootToMove.name}");
                }
                
                sceneView.Repaint();
            }
        }
    }
}
#endif