using DotsBridge.UI.Popups;
using UnityEngine;

namespace DotsBridge.UI
{
    /// <summary>
    /// Точка входа для регистрации всех UI-панелей уровня.
    /// </summary>
    public class PopupInstaller : MonoBehaviour
    {
        private void Start()
        {
            // Здесь мы регистрируем все наши независимые классы
            InstallPopups(
                new ElectricNodePopup()
                // new TemperatureSensorPopup(),
                // new AutomationCabinetPopup()
            );
        }

        private void InstallPopups(params IPopupDefinition[] definitions)
        {
            foreach (var definition in definitions)
            {
                PopupManager.Instance.RegisterDefinition(definition);
            }
            
            Debug.Log($"[DotsBridge UI] Успешно зарегистрировано {definitions.Length} типов всплывающих окон.");
        }
    }
}