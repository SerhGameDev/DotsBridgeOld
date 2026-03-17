using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Modules.Rotation
{
    public struct RotateTransformAxis : IComponentData
    {
        public float3 Value;
        public bool IsLocal; // Сохраняем выбор Local/World
    }

    // Скорость вращения в радианах/секунду
    public struct RotateTransformSpeed : IComponentData { public float Value; }
    
    // Тег-переключатель
    public struct IsTransformRotating : IComponentData, IEnableableComponent { }
    public struct HoveredTag : IComponentData { }

    public struct IsBeingMouseRotated : IComponentData, IEnableableComponent { }

    public struct MouseRotationConfig : IComponentData
    {
        public float Sensitivity;
        public bool UseInertia;   // Галочка инерции
        public float Friction;    // Трение (0.95 - плавно, 0.1 - мгновенно)
    }

    // Текущая накопленная скорость (для инерции)
    public struct MouseRotationVelocity : IComponentData
    {
        public float2 Value;
    }

    // Твой "Рубильник": если выключен — объект вообще не реагирует на мышь
    public struct CanBeMouseRotated : IComponentData, IEnableableComponent { }

    // Тег активного процесса (когда палец на кнопке)
    public struct IsCurrentlyDragging : IComponentData, IEnableableComponent { }

}