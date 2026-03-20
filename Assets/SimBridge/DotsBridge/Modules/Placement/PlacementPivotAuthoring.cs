using System;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace DotsBridge.Placement
{
    public class PlacementPivotAuthoring : MonoBehaviour
    {
        [Tooltip("Смещение от центра объекта (например, Y = 0.5 поднимет куб 1x1x1 так, чтобы он стоял на поверхности)")]
        public Vector3 Offset = new Vector3(0, 0.5f, 0); 
        
        public class PlacementPivotBaker : Baker<PlacementPivotAuthoring>
        {
            public override void Bake(PlacementPivotAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlacementPivot
                {
                    Value = authoring.Offset
                });
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;

            // Преобразуем локальный Offset в мировые координаты
            // transform.TransformPoint учитывает позицию, поворот и масштаб объекта
            Vector3 worldPivot = transform.TransformPoint(Offset);

            // Рисуем маленькую сферу в этой точке
            Gizmos.DrawSphere(worldPivot, 0.05f);

            // (Опционально) Рисуем линию от центра объекта до точки смещения
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, worldPivot);
        }
    }
}