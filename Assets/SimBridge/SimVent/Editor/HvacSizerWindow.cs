/*using SimVent.Authoring;
using SimVent.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SimVent.Editor
{
    public class HvacSizerWindow : OdinEditorWindow
    {
        [MenuItem("SimVent/HVAC Sizer Tool")]
        private static void OpenWindow()
        {
            GetWindow<HvacSizerWindow>("HVAC Калькулятор").Show();
        }

        [TitleGroup("Настройки помещения")]
        [Required("Укажите комнату, для которой ведем расчет")]
        public AirNodeAuthoring TargetRoom;

        [TitleGroup("Зимние условия (ГОСТ)")]
        [SuffixLabel("°C")] public float StreetTemp = -25f;
        [SuffixLabel("°C")] public float TargetRoomTemp = 20f;
        [Tooltip("Кратность воздухообмена. Жилые: 3, Кухни: 5-10")]
        [SuffixLabel("ACH")] public float AirChangesPerHour = 3f;

        [TitleGroup("Результат расчета")]
        [HideLabel, ShowInInspector, ReadOnly]
        public HvacCalculationResult CalculationResult;

        [Button("Рассчитать оборудование", ButtonSizes.Large)]
        [GUIColor(0.2f, 0.8f, 0.4f)]
        private void CalculateSystem()
        {
            if (TargetRoom == null)
            {
                Debug.LogWarning("Сначала выберите комнату!");
                return;
            }

            // Обращаемся к нашему математическому ядру
            CalculationResult = HvacCalculator.Calculate(TargetRoom, StreetTemp, TargetRoomTemp, AirChangesPerHour);

            Debug.Log($"<color=green>Расчет для {TargetRoom.gameObject.name} успешно выполнен!</color>");
        }
    }
}*/