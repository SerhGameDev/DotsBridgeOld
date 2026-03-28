using SimVent.Components;
using Unity.Entities;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SimVent.Authoring
{
    public class FanAuthoring : MonoBehaviour
    {
        [Title("Куда дуем (Труба)?")]
        [Required] public AirDuctAuthoring TargetDuct;

        [Title("Характеристики вентилятора")]
        [Tooltip("Максимальный физический напор, который может выдать этот вентилятор")]
        [SuffixLabel("Па")] public float MaxPressure = 500f; // <-- Добавил суффикс Па

        [Tooltip("Максимальный объем воздуха (для справки и будущих лимитов)")]
        [SuffixLabel("м³/ч")] public float MaxFlowCapacity = 5000f;

        [SuffixLabel("сек")] public float SpinUpTime = 5f;
        [SuffixLabel("сек")] public float SpinDownTime = 10f;

        // ==========================================
        // БЛОК АВТОМАТИЧЕСКОЙ НАСТРОЙКИ (ODIN)
        // ==========================================
        [FoldoutGroup("🛠 Автоподбор (по данным HVAC Sizer)")]
        [LabelText("Требуемый напор")]
        [Tooltip("Впишите сюда Required Pressure из калькулятора")]
        [SuffixLabel("Па")]
        public float TargetCalculatedPressure = 80f;

        [FoldoutGroup("🛠 Автоподбор (по данным HVAC Sizer)")]
        [LabelText("Требуемый расход")]
        [Tooltip("Впишите сюда Required Flow из калькулятора")]
        [SuffixLabel("м³/ч")]
        public float TargetCalculatedFlow = 225f;

        [FoldoutGroup("🛠 Автоподбор (по данным HVAC Sizer)")]
        [LabelText("Инженерный запас")]
        [Tooltip("На сколько процентов вентилятор должен быть мощнее минимума (обычно 20-30%)")]
        [PropertyRange(0f, 100f)]
        [SuffixLabel("%")]
        public float MarginPercent = 20f;

        [FoldoutGroup("🛠 Автоподбор (по данным HVAC Sizer)")]
        [Button("Применить к вентилятору", ButtonSizes.Medium)]
        [GUIColor(0.4f, 0.8f, 1f)]
        public float VisualRotationMultiplier = 10f;
        private void ApplyCalculatedSettings()
        {
            // Считаем множитель запаса (например, 20% = 1.2)
            float multiplier = 1f + (MarginPercent / 100f);

            // Применяем настройки к реальным характеристикам
            MaxPressure = TargetCalculatedPressure * multiplier;
            MaxFlowCapacity = TargetCalculatedFlow * multiplier;

            // Помечаем объект как "измененный", чтобы Unity сохранила сцену
            //UnityEditor.EditorUtility.SetDirty(this);

            Debug.Log($"<b>[Fan]</b> Настройки применены! Установлен напор: {MaxPressure:F1} Па, Расход: {MaxFlowCapacity:F1} м³/ч (С учетом запаса {MarginPercent}%)");
        }

        class Baker : Baker<FanAuthoring>
        {
            public override void Bake(FanAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new FanComponent
                {
                    TargetDuct = GetEntity(authoring.TargetDuct, TransformUsageFlags.None),
                    RunCommand = false,
                    CurrentSpeed = 0f,
                    SpinUpTime = authoring.SpinUpTime,
                    SpinDownTime = authoring.SpinDownTime,
                    MaxFlowCapacity = authoring.MaxFlowCapacity,
                    MaxPressure = authoring.MaxPressure
                });
            }
        }
    }
}