using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace SimElectric
{
    // Вешаем этот скрипт на префаб провода
    public class WireAuthoring : MonoBehaviour
    {
        [Title("Wire Connections", "Connect two electrical nodes")]

        [Required("Node A is required!")]
        public ElectricalNodeAuthoring nodeA;

        [Required("Node B is required!")]
        public ElectricalNodeAuthoring nodeB;

        [Title("Wire Properties")]
        [PropertyRange(0f, 100f)]
        public float maxCurrent = 16f; // Автомат на 16А по умолчанию
        [Title("Visuals")]
        public Color activeColor = Color.red;    // Цвет провода под напряжением
        public Color inactiveColor = Color.gray; // Цвет обесточенного провода

        // Кнопка для быстрой смены направления, если это важно для диодов
        [Button("Swap Nodes", ButtonSizes.Small)]
        private void SwapNodes()
        {
            (nodeA, nodeB) = (nodeB, nodeA);
        }

        public class WireBaker : Baker<WireAuthoring>
        {
            public override void Bake(WireAuthoring authoring)
            {
                if (authoring.nodeA == null || authoring.nodeB == null) return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Wire
                {
                    NodeA = GetEntity(authoring.nodeA, TransformUsageFlags.Dynamic),
                    NodeB = GetEntity(authoring.nodeB, TransformUsageFlags.Dynamic),
                    MaxCurrent = authoring.maxCurrent,
                    IsBroken = false,
                    IsConducting = true // <--- ОБЯЗАТЕЛЬНО ЭТА СТРОЧКА!
                });
            }
        }
    }
}