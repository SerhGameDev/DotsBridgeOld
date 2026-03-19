using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EditTool
{
    public enum SocketType
    {
        Default,
        Pipe,
        Ventilation,
        CableBox
    }

    [ExecuteAlways] // Важно добавить, чтобы OnDestroy работал в режиме Редактора
    public class EditToolSocket : MonoBehaviour
    {
        [TitleGroup("Настройки сокета")]
        [EnumToggleButtons]
        public SocketType socketType = SocketType.Default;

        [TitleGroup("Настройки сокета")]
        [Tooltip("Если на префабе несколько сокетов, этот флаг укажет, какой из них использовать для стыковки при спавне по умолчанию.")]
        public bool isMainSpawnPoint = false;

        [TitleGroup("Настройки иерархии")]
        [Tooltip("Корневой объект, который будет перемещаться и поворачиваться при стыковке. Если оставить пустым, будет двигаться сам объект с сокетом.")]
        public Transform rootTransform;

        [ShowInInspector, ReadOnly, TitleGroup("Состояние")]
        public EditToolSocket ConnectedSocket { get; private set; }

        public bool IsConnected => ConnectedSocket != null;

        public Vector3 Direction => transform.forward;

        [TitleGroup("Создание узла"), HideIf("IsConnected")]
        [Tooltip("Выберите префаб, который нужно присоединить к этой точке")]
        [AssetsOnly]
        public GameObject prefabToSpawn;

        [TitleGroup("Создание узла"), HideIf("IsConnected")]
        [Button("Присоединить объект", ButtonSizes.Medium), EnableIf("@prefabToSpawn != null")]
        
        private EditToolNode _parentNode;
        public EditToolNode ParentNode 
        {
            get 
            {
                // Ленивая инициализация: автоматически ищем менеджера выше по иерархии
                if (_parentNode == null) _parentNode = GetComponentInParent<EditToolNode>();
                return _parentNode;
            }
        }
        public void SpawnAndConnect()
        {
#if UNITY_EDITOR
            if (prefabToSpawn == null) return;

            GameObject spawnedObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
            Undo.RegisterCreatedObjectUndo(spawnedObj, "Spawn and Connect EditTool Object");

            EditToolSocket[] spawnedSockets = spawnedObj.GetComponentsInChildren<EditToolSocket>();
            
            EditToolSocket targetSocket = spawnedSockets.FirstOrDefault(s => s.isMainSpawnPoint && CanConnectTo(s));
            if (targetSocket == null)
            {
                targetSocket = spawnedSockets.FirstOrDefault(s => CanConnectTo(s));
            }

            if (targetSocket == null)
            {
                Debug.LogError($"[EditTool] На префабе {prefabToSpawn.name} нет подходящего свободного сокета типа {socketType}!");
                Undo.DestroyObjectImmediate(spawnedObj);
                return;
            }

            Transform rootToMove = targetSocket.rootTransform != null ? targetSocket.rootTransform : spawnedObj.transform;

            Quaternion desiredSocketRotation = Quaternion.LookRotation(-this.Direction, this.transform.up);
            Quaternion rotationDelta = desiredSocketRotation * Quaternion.Inverse(targetSocket.transform.rotation);
            rootToMove.rotation = rotationDelta * rootToMove.rotation;

            Vector3 positionDelta = this.transform.position - targetSocket.transform.position;
            rootToMove.position += positionDelta;

            Undo.RecordObject(this, "Connect Socket");
            Undo.RecordObject(targetSocket, "Connect Target Socket");
            
            this.Connect(targetSocket);

            prefabToSpawn = null;
            Selection.activeGameObject = rootToMove.gameObject;
#endif
        }

        public virtual bool CanConnectTo(EditToolSocket other)
        {
            if (other == null || other == this) return false;
            if (IsConnected || other.IsConnected) return false;
            if (this.socketType != other.socketType) return false;

            // ЗАЩИТА ОТ КОНФЛИКТА ВНУТРИ ОДНОЙ ДЕТАЛИ
            // Если оба сокета лежат внутри одного EditToolNode, они не могут соединиться
            if (this.ParentNode != null && other.ParentNode != null && this.ParentNode == other.ParentNode)
            {
                return false;
            }

            // Запасная защита, если менеджер не добавлен, но назначен одинаковый rootTransform
            if (this.rootTransform != null && other.rootTransform != null && this.rootTransform == other.rootTransform)
            {
                return false;
            }
            
            // Если rootTransform не назначен, но сокеты имеют общего прямого родителя
            if (this.transform.parent != null && this.transform.parent == other.transform.parent)
            {
                return false;
            }

            return true;
        }

        public void Connect(EditToolSocket other)
        {
            if (!CanConnectTo(other)) return;

            ConnectedSocket = other;
            other.ConnectedSocket = this;

            OnConnected(other);
            other.OnConnected(this);
        }

        [Button("Отключить"), ShowIf("IsConnected")]
        public void Disconnect()
        {
            if (!IsConnected) return;

            var other = ConnectedSocket;
            
#if UNITY_EDITOR
            if (!Application.isPlaying) 
            {
                Undo.RecordObject(this, "Disconnect Socket");
                if (other != null) Undo.RecordObject(other, "Disconnect Target Socket");
            }
#endif

            ConnectedSocket = null;
            if (other != null) other.ConnectedSocket = null;

            OnDisconnected(other);
            if (other != null) other.OnDisconnected(this);
        }

        protected virtual void OnConnected(EditToolSocket other) { }
        protected virtual void OnDisconnected(EditToolSocket other) { }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsConnected ? new Color(1f, 0f, 0f, 0.5f) : new Color(0f, 1f, 0f, 0.5f);
            
            if (isMainSpawnPoint) Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);
            else Gizmos.DrawSphere(transform.position, 0.1f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, Direction * 0.3f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsConnected ? Color.red : Color.green;
            if (isMainSpawnPoint) Gizmos.DrawWireCube(transform.position, Vector3.one * 0.12f);
            else Gizmos.DrawWireSphere(transform.position, 0.12f);
            
            if (rootTransform != null && rootTransform != transform)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, rootTransform.position);
            }
        }

        // ==========================================
        // ОЧИСТКА ПРИ УДАЛЕНИИ
        // ==========================================
        private void OnDestroy()
        {
            // Если объект удаляют из сцены, мы должны освободить соседний сокет
            if (IsConnected)
            {
                var other = ConnectedSocket;
                ConnectedSocket = null;
                if (other != null)
                {
                    other.Disconnect();
                }
            }
        }
    }
}