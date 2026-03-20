using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge.Character
{
    // === КОМПОНЕНТЫ ДАННЫХ ===

    /// <summary>
    /// Базовый тег для определения сущности как персонажа.
    /// </summary>
    public struct CharacterTag : IComponentData
    {
    }

    /// <summary>
    /// Тег активного персонажа, которым в данный момент управляет игрок.
    /// Сделан как IEnableableComponent для быстрого переключения управления (Possess/Unpossess).
    /// </summary>
    public struct ActiveCharacterTag : IComponentData, IEnableableComponent
    {
    }

    /// <summary>
    /// Хранит текущий ввод от игрока (WASD и мышь).
    /// </summary>
    public struct CharacterControlInput : IComponentData
    {
        public float2 MoveInput;
        public float2 LookInput;
    }
    /// <summary>
    /// Managed компонент. Хранит ссылку на созданную GameObject-камеру.
    /// </summary>
    public class CharacterCameraLink : IComponentData
    {
        public Camera Camera;
    }
    /// <summary>
    /// Настройки физики и перемещения персонажа.
    /// </summary>
    public struct CharacterSettings : IComponentData
    {
        public float MoveSpeed;
        public float LookSpeed;
        public float StepHeight;
        public float CharacterRadius;
        public float Gravity;
        public float EyeHeight; 
    }
    /// <summary>
    /// Хранит текущий угол наклона головы (Pitch), чтобы камера могла смотреть вверх/вниз.
    /// Вращение влево/вправо (Yaw) будет применяться к самому LocalTransform сущности.
    /// </summary>
    public struct CharacterViewState : IComponentData
    {
        public float Pitch; 
    }
    /// <summary>
    /// Текущий вектор скорости и состояние нахождения на земле.
    /// </summary>
    public struct CharacterVelocity : IComponentData
    {
        public float3 Value;
        public bool IsGrounded;
    }
}