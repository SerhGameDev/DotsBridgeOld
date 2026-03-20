using Unity.Entities;
using Unity.Mathematics;

namespace DotsBridge.Interaction
{
    public struct HingeState : IComponentData
    {
        public float CurrentState; // Текущее положение от 0 до 1
        public float TargetState;  // Целевое положение от 0 до 1
        public float Speed;        // Скорость открытия
        
        public quaternion ClosedRotation; // Поворот при 0
        public quaternion OpenRotation;   // Поворот при 1
    }
}