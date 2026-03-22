using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Sirenix.OdinInspector; // Подключаем пространство имен Odin

namespace DotsBridge.Interaction
{
    public class HingeAuthoring : MonoBehaviour
    {
        [BoxGroup("Настройки углов")]
        [Tooltip("Углы поворота в закрытом состоянии (State = 0)")]
        [OnValueChanged("UpdatePreview")] // Автоматически обновляет предпросмотр при изменении значений
        public Vector3 ClosedEulerAngles = Vector3.zero;
        
        [BoxGroup("Настройки углов")]
        [Tooltip("Углы поворота в открытом состоянии (State = 1)")]
        [OnValueChanged("UpdatePreview")]
        public Vector3 OpenEulerAngles = new Vector3(0, 90f, 0);
        
        [BoxGroup("Параметры анимации")]
        [Tooltip("Скорость анимации (единиц State в секунду)")]
        public float Speed = 5f;

        [BoxGroup("Параметры анимации")]
        [Tooltip("Начальное состояние (например, сразу открыто)")]
        [Range(0f, 1f)] 
        [OnValueChanged("UpdatePreview")] // Двигая ползунок в редакторе, дверь будет плавно открываться/закрываться
        public float InitialState = 0f;

        // --- ИНСТРУМЕНТЫ ODIN INSPECTOR ---

        [ButtonGroup("Предпросмотр")]
        [Button("Смотреть Закрыто (0)", ButtonSizes.Medium)]
        private void PreviewClosed()
        {
            InitialState = 0f;
            UpdatePreview();
        }

        [ButtonGroup("Предпросмотр")]
        [Button("Смотреть Открыто (1)", ButtonSizes.Medium)]
        private void PreviewOpen()
        {
            InitialState = 1f;
            UpdatePreview();
        }

        [ButtonGroup("Сохранение")]
        [Button("Сохранить текущий как Закрыто", ButtonSizes.Small)]
        private void SaveCurrentAsClosed()
        {
            ClosedEulerAngles = transform.localEulerAngles;
        }

        [ButtonGroup("Сохранение")]
        [Button("Сохранить текущий как Открыто", ButtonSizes.Small)]
        private void SaveCurrentAsOpen()
        {
            OpenEulerAngles = transform.localEulerAngles;
        }

        // Метод для интерполяции вращения объекта в редакторе
        private void UpdatePreview()
        {
            if (transform != null)
            {
                // Используем Lerp для вычисления промежуточного угла в зависимости от InitialState
                transform.localEulerAngles = Vector3.Lerp(ClosedEulerAngles, OpenEulerAngles, InitialState);
            }
        }

        // --- DOTS BAKER ---

        public class HingeBaker : Baker<HingeAuthoring>
        {
            public override void Bake(HingeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                foreach (Transform child in authoring.transform)
                {
                    GetEntity(child, TransformUsageFlags.Dynamic);
                }

                quaternion closedRot = quaternion.Euler(math.radians(authoring.ClosedEulerAngles));
                quaternion openRot = quaternion.Euler(math.radians(authoring.OpenEulerAngles));

                AddComponent(entity, new HingeState
                {
                    CurrentState = authoring.InitialState,
                    TargetState = authoring.InitialState,
                    Speed = authoring.Speed,
                    ClosedRotation = closedRot,
                    OpenRotation = openRot
                });
            }
        }
    }
}