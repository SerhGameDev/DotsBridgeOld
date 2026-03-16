using Unity.Entities;
using UnityEngine;
using SimVent.Components;

namespace SimVent.Authoring
{
    public class NodeTemperatureSensorAuthoring : MonoBehaviour
    {
        [Header("Привязка")]
        [Tooltip("Комната или узел, температуру которого мы меряем")]
        public AirNodeAuthoring TargetNode;

        [Header("Настройки")]
        [Tooltip("Постоянная времени (сек). 0 - мгновенно, 5-10 - реалистичная задержка корпуса")]
        public float SensorTimeConstant = 5f;

        [Header("Диагностика")]
        public float CurrentMeasuredTemp;

        class Baker : Baker<NodeTemperatureSensorAuthoring>
        {
            public override void Bake(NodeTemperatureSensorAuthoring authoring)
            {
                if (authoring.TargetNode == null) return;

                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new NodeTemperatureSensorComponent
                {
                    TargetNode = GetEntity(authoring.TargetNode, TransformUsageFlags.None),
                    MeasuredTemperature = 20f, // Начальное значение
                    SensorTimeConstant = authoring.SensorTimeConstant
                });
            }
        }
    }
}