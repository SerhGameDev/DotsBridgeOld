using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge
{
    public class ToggleRotationAuthoring : MonoBehaviour
    {
        public bool isOn = true;
        public float speedDegrees = 180f; // В инспекторе удобнее задавать в градусах
        public Vector3 axis = Vector3.forward;

        class Baker : Baker<FanRotationAuthoring>
        {
            public override void Bake(FanRotationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
            
                AddComponent(entity, new FanRotation
                {
                    IsOn = authoring.isOn,
                    Speed = math.radians(authoring.speedDegrees), // Конвертируем в радианы для DOTS
                    Axis = math.normalize(authoring.axis)
                });
            }
        }
    }
}