using UnityEngine;
using UnityEngine.UI;
using SimVent.Components;
using DotsBridge;
using Unity.Entities;
using TMPro;

namespace SimVent.UI
{
    public class VentSystemUI : MonoBehaviour
    {
        [Header("Управление двигателями")]
        public Button ToggleFanButton;
        public Button DumpControllButton;
        public TextMeshProUGUI FanButtonText;
        public TextMeshProUGUI DumpControllStatusText;

        [Header("Управление ТЭНом")]
        public Button ToggleHeaterButton;
        public TextMeshProUGUI HeaterButtonText;
        public Slider TenSlider;

        [Header("Индикация датчиков")]
        public TextMeshProUGUI DpsStatusText;
        public Image DpsIndicatorLight;

        [Header("Индикация температуры")]
        public TextMeshProUGUI TemperatureText;
        public TextMeshProUGUI OverheatAlarmText;
        public Image OverheatIndicatorLight;

        [Header("Цвета индикатора")]
        public Color ColorOff = Color.gray;
        public Color ColorOn = Color.green;
        public Color ColorAlarm = Color.red;

        private bool _isFanRunning = false;
        private bool _isDumpOpen = false;
        private bool _isHeaterOn = false;

        private void Start()
        {
            DumpControllButton.onClick.AddListener(OnOpenDump);
            ToggleFanButton.onClick.AddListener(OnToggleFanClicked);
            ToggleHeaterButton.onClick.AddListener(OnToggleHeaterClicked);

            UpdateUIState();
        }

        private void OnOpenDump()
        {
            _isDumpOpen = !_isDumpOpen;
            UpdateUIState();

            using (var bridgeList = ClientBridge.World().FindWithComponent<DamperComponent>())
            {
                for (int i = 0; i < bridgeList.Count; i++)
                {
                    var entity = bridgeList.Entities[i];
                    var dump = bridgeList.Manager.GetComponentData<DamperComponent>(entity);
                    dump.TargetOpening = _isDumpOpen ? 1 : 0;
                    bridgeList.Manager.SetComponentData(entity, dump);
                }
            }
        }

        private void OnToggleFanClicked()
        {
            _isFanRunning = !_isFanRunning;
            UpdateUIState();

            using (var bridgeList = ClientBridge.World().FindWithComponent<FanComponent>())
            {
                for (int i = 0; i < bridgeList.Count; i++)
                {
                    var entity = bridgeList.Entities[i];
                    var fan = bridgeList.Manager.GetComponentData<FanComponent>(entity);
                    fan.RunCommand = _isFanRunning;
                    bridgeList.Manager.SetComponentData(entity, fan);
                }
            }
        }

        private void OnToggleHeaterClicked()
        {
            // Если ТЭН в аварии, то первое нажатие просто сбрасывает аварию, но не включает нагрев
            using (var bridgeList = ClientBridge.World().FindWithComponent<ElectricHeaterComponent>())
            {
                for (int i = 0; i < bridgeList.Count; i++)
                {
                    var entity = bridgeList.Entities[i];
                    var heater = bridgeList.Manager.GetComponentData<ElectricHeaterComponent>(entity);

                    if (heater.IsOverheated)
                    {
                        heater.IsOverheated = false; // СБРОС АВАРИИ
                        Debug.Log("Авария ТЭН сброшена пользователем");
                    }
                    else
                    {
                        _isHeaterOn = !_isHeaterOn; // Обычное переключение, если нет аварии
                    }

                    heater.TargetPower = _isHeaterOn ? 0.2f : 0.0f;
                    bridgeList.Manager.SetComponentData(entity, heater);
                }
            }
            UpdateUIState();
        }

        private void UpdateUIState()
        {
            DumpControllStatusText.text = _isDumpOpen ? "ЗАКРЫТЬ ЗАСЛОНКУ" : "ОТКРЫТЬ ЗАСЛОНКУ";
            FanButtonText.text = _isFanRunning ? "ВЫКЛЮЧИТЬ ВЕНТИЛЯТОР" : "ВКЛЮЧИТЬ ВЕНТИЛЯТОР";
            HeaterButtonText.text = _isHeaterOn ? "ВЫКЛЮЧИТЬ ТЭН" : "ВКЛЮЧИТЬ ТЭН";
        }

        private void Update()
        {
            var bridgeWorld = ClientBridge.World();

            // 1. Читаем DPS (Расход воздуха)
            using (var dpsList = bridgeWorld.FindWithComponent<DpsSensorComponent>())
            {
                if (dpsList.Count > 0)
                {
                    var dps = dpsList.Manager.GetComponentData<DpsSensorComponent>(dpsList.Entities[0]);
                    DpsStatusText.text = dps.OutputSignal ? "Датчик потока: ЗАМКНУТ" : "Датчик потока: РАЗОМКНУТ";
                    DpsIndicatorLight.color = dps.OutputSignal ? ColorOn : ColorOff;
                }
            }

            // 2. Читаем Температуру
            using (var tempList = bridgeWorld.FindWithComponent<TemperatureSensorComponent>())
            {
                if (tempList.Count > 0)
                {
                    var sensor = tempList.Manager.GetComponentData<TemperatureSensorComponent>(tempList.Entities[0]);
                    TemperatureText.text = $"Температура: {sensor.MeasuredTemperature:F1} °C";
                }
            }
            // 3. Читаем состояние ТЭНа (Авария)
            using (var heaterList = bridgeWorld.FindWithComponent<ElectricHeaterComponent>())
            {
                if (heaterList.Count > 0)
                {
                    var heater = heaterList.Manager.GetComponentData<ElectricHeaterComponent>(heaterList.Entities[0]);

                    if (heater.IsOverheated)
                    {
                        OverheatAlarmText.text = "АВАРИЯ: ПЕРЕГРЕВ ТЭН!";
                        OverheatIndicatorLight.color = ColorAlarm; // Красный
                    }
                    else
                    {
                        OverheatAlarmText.text = "ТЭН Норма";
                        OverheatIndicatorLight.color = ColorOff; // Серый
                    }
                }
            }
        }
    }
}