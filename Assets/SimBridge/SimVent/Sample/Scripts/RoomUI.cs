using DotsBridge;
using SimVent.Authoring;
using SimVent.Components;
using TMPro;
using UnityEngine;

public class RoomUI : MonoBehaviour
{
    public TextMeshProUGUI _roomTemperatureText;
    private void Update()
    {
        using (var roomList = EntityBridge.InCurrentWorld().FindWithComponent<NodeTemperatureSensorComponent>())
        {
            if (roomList.Count > 0)
            {
                var room = roomList.Manager.GetComponentData<NodeTemperatureSensorComponent>(roomList.Entities[0]);
                _roomTemperatureText.text = $"Температура в помещении: {room.MeasuredTemperature:F1} °C";
            }
        }
    }
}
