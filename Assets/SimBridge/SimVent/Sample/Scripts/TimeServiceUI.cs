using UnityEngine;
using UnityEngine.UI;
using DotsBridge;
using SimBridge.Core.Time;
using TMPro;

namespace SimBridge.UI
{
    public class TimeServiceUI : MonoBehaviour
    {
        [Header("Настройки времени")]
        public Slider TimeScaleSlider;
        public TextMeshProUGUI TimeScaleText;
        public TextMeshProUGUI TotalSimulationTimeText;

        private void Start()
        {
            TimeScaleSlider.minValue = 0f;
            TimeScaleSlider.maxValue = 15f; // Ускорение до х100

            TimeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
            TimeScaleSlider.value = 1f;
        }

        private void OnTimeScaleChanged(float value)
        {
            // 1. МАГИЯ UNITY: Ускоряем всё время в движке
            // Unity сама начнет вызывать физику в X раз чаще, не ломая математику!
            UnityEngine.Time.timeScale = value;

            // 2. Опционально: сохраняем в компонент, если он вам нужен для других целей
            using (var bridge = EntityBridge.InCurrentWorld().FindWithComponent<SimulationTimeComponent>())
            {
                if (bridge.Count > 0)
                {
                    var timeData = bridge.Manager.GetComponentData<SimulationTimeComponent>(bridge.Entities[0]);
                    timeData.TimeScale = value;
                    bridge.Manager.SetComponentData(bridge.Entities[0], timeData);
                }
            }

            TimeScaleText.text = $"x{value:F1}";
        }
        private void Update()
        {
            // Отображаем общее время симуляции (сколько "виртуальных" часов прошло)
            using (var bridge = EntityBridge.InCurrentWorld().FindWithComponent<SimulationTimeComponent>())
            {
                if (bridge.Count > 0)
                {
                    var timeData = bridge.Manager.GetComponentData<SimulationTimeComponent>(bridge.Entities[0]);

                    // Форматируем секунды в часы:минуты:секунды
                    System.TimeSpan t = System.TimeSpan.FromSeconds(timeData.TotalTime);
                    TotalSimulationTimeText.text = $"Системное время: {t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
                }
            }
        }
    }
}