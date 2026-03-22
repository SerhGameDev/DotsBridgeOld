using SimVent.Authoring;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace SimVent.Core
{
    [System.Serializable]
    public class HvacCalculationResult
    {
        [BoxGroup("Параметры вентилятора", centerLabel: true)]
        [LabelText("Требуемый расход")]
        [Tooltip("Объем воздуха, который вентилятор должен прокачивать за один час, чтобы обеспечить нужную кратность воздухообмена (ACH) с учетом всех утечек из комнаты.")]
        [SuffixLabel("м³/ч")]
        public float RequiredFlow_m3h;

        [BoxGroup("Параметры вентилятора")]
        [LabelText("Потери в трассе (Трубы)")]
        [Tooltip("Аэродинамическое сопротивление самих труб. Считается на основе длины транзитных узлов (Volume) и количества соединений.")]
        [SuffixLabel("Па")]
        public float PaDuctLoss;

        [BoxGroup("Параметры вентилятора")]
        [LabelText("Требуемый напор")]
        [Tooltip("Итоговое давление вентилятора. Включает потери в трубах + запас 50 Па на оборудование (фильтры, решетки, нагреватели).")]
        [GUIColor(0.4f, 0.8f, 1f)]
        [SuffixLabel("Па")]
        public float RequiredPressure_Pa;

        [BoxGroup("Тепловой баланс (Мощность ТЭНа)", centerLabel: true)]
        [LabelText("Нагрев притока")]
        [Tooltip("Энергия, которая тратится ИСКЛЮЧИТЕЛЬНО на то, чтобы нагреть ледяной воздух с улицы до нужной температуры. (Основная часть нагрузки)")]
        [SuffixLabel("кВт")]
        public float KwFreshAir;

        [BoxGroup("Тепловой баланс (Мощность ТЭНа)")]
        [LabelText("Потери помещения")]
        [Tooltip("Дополнительная энергия для компенсации тепла, которое комната постоянно отдает на улицу через свои стены (зависит от Heat Loss Factor).")]
        [SuffixLabel("кВт")]
        public float KwRoomLoss;

        [BoxGroup("Тепловой баланс (Мощность ТЭНа)")]
        [LabelText("Потери в трассе")]
        [Tooltip("Дополнительная энергия для компенсации остывания горячего воздуха, пока он идет по транзитным холодным трубам до комнаты.")]
        [SuffixLabel("кВт")]
        public float KwDuctLoss;

        [BoxGroup("Итоговый результат", centerLabel: true)]
        [LabelText("Физический минимум")]
        [Tooltip("Абсолютный математический минимум мощности ТЭНа. При этой мощности система будет работать на пределе своих возможностей без права на ошибку.")]
        [SuffixLabel("кВт")]
        public float TotalRequired_kW;

        [BoxGroup("Итоговый результат")]
        [LabelText("Рекомендуемый ТЭН (с запасом)")]
        [Tooltip("Мощность ТЭНа с добавленным инженерным запасом прочности (20%). Эту цифру следует вписывать в поле Max Power KW в настройках нагревателя.")]
        [GUIColor(1f, 0.4f, 0.4f)]
        [SuffixLabel("кВт")]
        public float RecommendedHeater_kW;
    }

    public static class HvacCalculator
    {
        public static HvacCalculationResult Calculate(AirNodeAuthoring targetRoom, float streetTemp, float targetRoomTemp, float ach)
        {
            if (targetRoom == null) return null;

            var result = new HvacCalculationResult();

            // 1. Поиск трассы до улицы
            List<AirNodeAuthoring> routeNodes = new List<AirNodeAuthoring>();
            int ductCount = 0;

            AirNodeAuthoring currentNode = targetRoom;
            AirDuctAuthoring[] allDucts = Object.FindObjectsByType<AirDuctAuthoring>(FindObjectsSortMode.None);

            int safeLimit = 100;
            while (currentNode != null && !currentNode.IsInfinite && safeLimit > 0)
            {
                routeNodes.Add(currentNode);
                AirNodeAuthoring nextNode = null;

                foreach (var duct in allDucts)
                {
                    if (duct.TargetNode == currentNode)
                    {
                        ductCount++;
                        nextNode = duct.SourceNode;
                        break;
                    }
                }
                currentNode = nextNode;
                safeLimit--;
            }

            // 2. РАСЧЕТ ВЕНТИЛЯТОРА И АЭРОДИНАМИКИ
            float leakMultiplier = 1f + Mathf.Clamp01(targetRoom.LeakFactor);
            result.RequiredFlow_m3h = (targetRoom.Volume * ach) * leakMultiplier;

            // Вычисляем общую длину трассы по Volume транзитных узлов
            float totalDuctLength = 0f;
            foreach (var node in routeNodes)
            {
                if (node != targetRoom) totalDuctLength += node.Volume;
            }

            // Реальная физика (СНиП):
            // Потеря на трение в гладкой трубе ~ 1.5 Па на 1 метр длины
            // Потеря на каждом местном сопротивлении (соединение, поворот) ~ 5 Па
            float frictionLoss = totalDuctLength * 1.5f;
            float localLoss = ductCount * 5.0f;

            result.PaDuctLoss = frictionLoss + localLoss;

            // Итоговый напор: потери труб + запас 50 Па на ТЭН/Заслонки
            result.RequiredPressure_Pa = result.PaDuctLoss + 50f;

            // 3. РАСЧЕТ ТЕПЛОТЕХНИКИ (ТЭН)
            float deltaT = targetRoomTemp - streetTemp;

            // Нагрев воздуха: Q * dT / 2985
            result.KwFreshAir = (result.RequiredFlow_m3h * deltaT) / 2985f;

            // Потери комнаты
            result.KwRoomLoss = (targetRoom.Volume * targetRoom.HeatLossFactor * deltaT) * 0.05f;

            // Потери в магистрали
            result.KwDuctLoss = 0f;
            foreach (var node in routeNodes)
            {
                if (node != targetRoom)
                {
                    result.KwDuctLoss += (node.Volume * node.HeatLossFactor * deltaT) * 0.05f;
                }
            }

            result.TotalRequired_kW = result.KwFreshAir + result.KwRoomLoss + result.KwDuctLoss;
            result.RecommendedHeater_kW = result.TotalRequired_kW * 1.2f; // Запас 20%

            return result;
        }
    }
}