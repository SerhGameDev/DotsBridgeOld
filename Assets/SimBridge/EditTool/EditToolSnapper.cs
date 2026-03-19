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
        private static EditToolSocket[] allSocketsInScene; 

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
            bool justStartedDragging = (lastHotControl == 0 && currentHotControl != 0); 
            bool justDropped = (lastHotControl != 0 && currentHotControl == 0); 
            lastHotControl = currentHotControl;

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
                // ==========================================
                // АВТО-ОТКЛЮЧЕНИЕ ПРИ РАЗРЫВЕ ДИСТАНЦИИ
                // ==========================================
                if (moving.IsConnected)
                {
                    var partner = moving.ConnectedSocket;
                    if (partner != null)
                    {
                        float dist = Vector3.Distance(moving.transform.position, partner.transform.position);
                        float angle = Vector3.Angle(moving.Direction, -partner.Direction);

                        // Если растащили на 5 см или повернули на 3 градуса — рвем связь
                        if (dist > 0.05f || angle > 3f)
                        {
                            Undo.RecordObject(moving, "Auto Disconnect");
                            Undo.RecordObject(partner, "Auto Disconnect Target");
                            moving.Disconnect();
                            Debug.Log($"[EditTool] Связь разорвана при перемещении: {moving.name}");
                        }
                    }
                }

                // Если после проверки выше он все еще занят (значит не двигали) - пропускаем
                if (moving.IsConnected) continue;
             
                // Поиск новых соединений
                foreach (var target in allSocketsInScene)
                {
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
                    Transform rootToMove = bestMoving.rootTransform != null ? bestMoving.rootTransform : 
                        (bestMoving.ParentNode != null ? bestMoving.ParentNode.transform : activeObj.transform);

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