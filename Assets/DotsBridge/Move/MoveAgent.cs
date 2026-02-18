using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge
{
    public struct MoveAgent : IComponentData
    {
        public float Speed;
        public float RotationSpeed; 
        public float StoppingDistance; 
    }


    // Тег, говорящий "Я сейчас двигаюсь" (для анимаций или логики)
    public struct IsMovingTag : IComponentData, IEnableableComponent { }
    // --- ДАННЫЕ (Параметры) ---

    // Скорость передвижения
    public struct MoveSpeed : IComponentData { public float Value; }

    // Скорость вращения
    public struct RotationSpeed : IComponentData { public float Value; } 

    // Дистанция остановки (для движения к цели)
    public struct StopDistance : IComponentData { public float Value; }


    // --- ЦЕЛИ (Куда?) ---

    // Идти в направлении (Vector)
    public struct MoveDirection : IComponentData { public float3 Value; }

    // Идти в точку (Coordinate)
    public struct MoveTarget : IComponentData { public float3 Value; }


    // --- ТЕГИ (Логика: Как?) ---

    // Двигать через Transform (Телепортация/Кинематика)
    public struct MoveWithTransformTag : IComponentData, IEnableableComponent { }

    // Двигать через PhysicsVelocity (Силы/Скорость)
    public struct MoveWithPhysicsTag : IComponentData, IEnableableComponent { }

    // Вращаться в сторону движения
    public struct RotateToMovementTag : IComponentData, IEnableableComponent { }
}