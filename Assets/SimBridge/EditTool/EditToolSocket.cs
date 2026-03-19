using UnityEngine;
using Sirenix.OdinInspector;

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

    public class EditToolSocket : MonoBehaviour
    {
        [Title("Настройки сокета")]
        [EnumToggleButtons]
        public SocketType socketType = SocketType.Default;

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
        public void SpawnAndConnect()
        {
#if UNITY_EDITOR
            if (prefabToSpawn == null) return;

            // 1. Создаем объект как инстанс префаба и регистрируем для Ctrl+Z
            GameObject spawnedObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
            Undo.RegisterCreatedObjectUndo(spawnedObj, "Spawn and Connect EditTool Object");

            // 2. Ищем встречный свободный сокет подходящего типа на созданном объекте
            EditToolSocket[] spawnedSockets = spawnedObj.GetComponentsInChildren<EditToolSocket>();
            EditToolSocket targetSocket = null;

            foreach (var s in spawnedSockets)
            {
                if (CanConnectTo(s))
                {
                    targetSocket = s;
                    break;
                }
            }

            if (targetSocket == null)
            {
                Debug.LogError($"[EditTool] На префабе {prefabToSpawn.name} нет подходящего свободного сокета типа {socketType}!");
                Undo.DestroyObjectImmediate(spawnedObj);
                return;
            }

            // 3. МАТЕМАТИКА ВЫРАВНИВАНИЯ (Ориентация и Позиция)
            Transform rootTransform = spawnedObj.transform;

            // Шаг А: Поворот. Целевой сокет должен смотреть прямо противоположно нашему.
            // Используем Up-вектор текущего сокета, чтобы деталь не перекрутило по оси Z.
            Quaternion desiredSocketRotation = Quaternion.LookRotation(-this.Direction, this.transform.up);
            
            // Вычисляем разницу между текущим поворотом целевого сокета и желаемым
            Quaternion rotationDelta = desiredSocketRotation * Quaternion.Inverse(targetSocket.transform.rotation);
            
            // Применяем этот поворот ко всему созданному объекту
            rootTransform.rotation = rotationDelta * rootTransform.rotation;

            // Шаг Б: Позиция. Двигаем весь объект так, чтобы позиции сокетов совпали.
            Vector3 positionDelta = this.transform.position - targetSocket.transform.position;
            rootTransform.position += positionDelta;

            // 4. Фиксируем изменения в Undo и соединяем
            Undo.RecordObject(this, "Connect Socket");
            Undo.RecordObject(targetSocket, "Connect Target Socket");
            
            this.Connect(targetSocket);

            // Очищаем поле, чтобы интерфейс переключился, и выделяем новый объект для удобства
            prefabToSpawn = null;
            Selection.activeGameObject = spawnedObj;
#endif
        }

        public virtual bool CanConnectTo(EditToolSocket other)
        {
            if (other == null || other == this) return false;
            if (IsConnected || other.IsConnected) return false;
            if (this.socketType != other.socketType) return false;

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
            
            // Записываем разрыв соединения в историю Undo
#if UNITY_EDITOR
            Undo.RecordObject(this, "Disconnect Socket");
            if (other != null) Undo.RecordObject(other, "Disconnect Target Socket");
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
            Gizmos.DrawSphere(transform.position, 0.1f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, Direction * 0.3f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsConnected ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.12f);
        }
    }
}