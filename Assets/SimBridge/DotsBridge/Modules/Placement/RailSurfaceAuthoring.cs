using Unity.Entities;

namespace DotsBridge.Placement
{

    // Вешаем этот скрипт на префаб/объект DIN-рейки
    public class RailSurfaceAuthoring : UnityEngine.MonoBehaviour
    {
        [UnityEngine.Tooltip("Вдоль какой оси рейки можно двигать реле? Обычно это X")]
        public RailAxis Axis = RailAxis.X;
        
        public class RailSurfaceBaker : Baker<RailSurfaceAuthoring>
        {
            public override void Bake(RailSurfaceAuthoring authoring)
            {
                var entity = GetEntity(Unity.Entities.TransformUsageFlags.Dynamic);
                AddComponent(entity, new RailSurface { AllowedAxis = authoring.Axis });
            }
        }
    }

}