using Unity.Entities;
using UnityEngine;

namespace SimOil.Tests
{
    public class FluidSpawnerAuthoring : MonoBehaviour
    {
        public GameObject NodePrefab; // Сюда закинь твой префаб резервуара
        public GameObject LinkPrefab; // Сюда закинь твой префаб трубы

        class Baker : Baker<FluidSpawnerAuthoring>
        {
            public override void Bake(FluidSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                
                // Сохраняем ссылки на префабы для сервера
                AddComponent(entity, new FluidSpawnerConfig
                {
                    NodePrefab = GetEntity(authoring.NodePrefab, TransformUsageFlags.Dynamic),
                    LinkPrefab = GetEntity(authoring.LinkPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}