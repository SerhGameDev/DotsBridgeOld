using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EditTool
{
    [DisallowMultipleComponent]
    public class EditToolNode : MonoBehaviour
    {
        [Title("Менеджер детали")]
        [InfoBox("Этот компонент объединяет все вложенные сокеты в единую деталь. Сокеты внутри этого объекта не будут соединяться друг с другом.")]
        
        [ShowInInspector, ReadOnly]
        public EditToolSocket[] ChildSockets => GetComponentsInChildren<EditToolSocket>();

        // ==========================================
        // СИСТЕМА ЗАМЕНЫ ДЕТАЛИ (SWAP)
        // ==========================================
        
        [TitleGroup("Замена детали")]
        [Tooltip("Префаб, на который нужно заменить текущую деталь")]
        [AssetsOnly]
        public GameObject replacementPrefab;

        [TitleGroup("Замена детали")]
        [Button("Заменить деталь", ButtonSizes.Large), EnableIf("@replacementPrefab != null")]
        public void Replace()
        {
#if UNITY_EDITOR
            if (replacementPrefab == null) return;

            // 1. Запоминаем всех соседей и отключаемся от них
            List<EditToolSocket> oldNeighbors = new List<EditToolSocket>();
            foreach (var socket in ChildSockets)
            {
                if (socket.IsConnected)
                {
                    oldNeighbors.Add(socket.ConnectedSocket);
                    socket.Disconnect(); // Корректно разрываем связь с Undo
                }
            }

            // 2. Создаем новую деталь на том же самом месте
            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefab);
            Undo.RegisterCreatedObjectUndo(newObj, "Replace EditTool Node");
            
            newObj.transform.position = this.transform.position;
            newObj.transform.rotation = this.transform.rotation;
            newObj.transform.localScale = this.transform.localScale;

            // 3. Пытаемся восстановить связи для новой детали
            EditToolSocket[] newSockets = newObj.GetComponentsInChildren<EditToolSocket>();
            
            foreach (var neighbor in oldNeighbors)
            {
                // Ищем свободный сокет на новой детали, который находится в той же точке пространства, что и сосед
                var bestMatch = newSockets.FirstOrDefault(ns => 
                    !ns.IsConnected && 
                    ns.CanConnectTo(neighbor) && 
                    Vector3.Distance(ns.transform.position, neighbor.transform.position) < 0.1f); // Погрешность 10 см на случай мелких неточностей префаба

                if (bestMatch != null)
                {
                    Undo.RecordObject(bestMatch, "Reconnect Replacement");
                    Undo.RecordObject(neighbor, "Reconnect Target");
                    bestMatch.Connect(neighbor);
                }
            }

            // 4. Уничтожаем старую деталь
            Undo.DestroyObjectImmediate(this.gameObject);
            
            // 5. Выделяем новую деталь, чтобы продолжить с ней работу
            Selection.activeGameObject = newObj;
#endif
        }
    }
}