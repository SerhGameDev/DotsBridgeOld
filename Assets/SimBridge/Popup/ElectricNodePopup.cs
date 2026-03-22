using DotsBridge.UI.Positioning;
using SimElectric; // Твой неймспейс
using UnityEngine;

namespace DotsBridge.UI.Popups
{
    public class ElectricNodePopup : IPopupDefinition
    {
        public bool CanHandle(SingleEntity entity) 
        {
            return entity.HasComponent<ElectricalNode>();
        }

        public void BuildContent(SingleEntity entity, PopupBuilder builder)
        {
            var node = entity.GetComponent<ElectricalNode>();
            
            builder.AddHeader("Клемма питания")
                .AddValue("Напряжение", node.CurrentVoltage, "V")
                .AddValue("Сопротивление", node.Resistance, "Ω")
                .AddStatus("Заземлен", node.IsGrounded);
        }

        public IPopupPositionStrategy GetStrategy()
        {
            // У каждого окна может быть своя конфигурация отступов
            return new MouseCursorStrategy { Offset = new Vector2(15, -15) }; 
        }
    }
}