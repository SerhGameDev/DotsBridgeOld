using DotsBridge.Build;
using Unity.Entities;
using UnityEngine;

namespace DotsBridge.Authoring
{
    public class WallAuthoring : DotsEntityAuthoring
    {
        class Baker : Baker<WallAuthoring>
        {
            public override void Bake(WallAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                // Добавляем тег и данные стены
                AddComponent<WallTag>(entity);
                AddComponent<WallComponent>(entity);
                
                // Добавляем буфер для связей
                AddBuffer<ConnectionElement>(entity);
            }
        }
    }
}