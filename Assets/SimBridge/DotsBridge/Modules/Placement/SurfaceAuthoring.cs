using Unity.Entities;
using UnityEngine;
using DotsBridge.Placement;

namespace DotsBridge
{
    // 1. Вешаем этот скрипт на куб (пол/стену/дверцу шкафа), на который будем ставить объекты.
    public class SurfaceAuthoring : MonoBehaviour
    {
        
        public class SurfaceBaker : Baker<SurfaceAuthoring>
        {
            public override void Bake(SurfaceAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent<SurfaceTag>(entity);
            }
        }
    }


    // 2. Структура для хранения префаба в ECS
    public struct PlacementSettings : IComponentData
    {
        public Entity GhostPrefab;
    }

    // 3. Вешаем этот скрипт на пустой GameObject на сцене, чтобы передать префаб реле/кнопки в DOTS.
}