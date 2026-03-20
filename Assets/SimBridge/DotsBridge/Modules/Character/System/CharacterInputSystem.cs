using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge.Character
{
    /// <summary>
    /// Система считывания ввода. Обновляется в InitializationSystemGroup, 
    /// чтобы данные ввода были готовы до начала физических расчетов в SimulationSystemGroup.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class CharacterInputSystem : SystemBase
    {
        protected override void OnCreate()
        {
            // Система работает только если есть хотя бы один активный персонаж
            RequireForUpdate<ActiveCharacterTag>();
        }

        protected override void OnUpdate()
        {
            // Считываем сырой ввод (здесь используется классический Input Manager для простоты)
            // При необходимости легко заменяется на новую Input System
            float2 moveInput = new float2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            float2 lookInput = new float2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Нормализуем вектор движения, чтобы по диагонали скорость не была выше
            if (math.lengthsq(moveInput) > 1f)
            {
                moveInput = math.normalize(moveInput);
            }

            // Записываем ввод ТОЛЬКО в активных персонажей
            foreach (var input in SystemAPI.Query<RefRW<CharacterControlInput>>()
                         .WithAll<CharacterTag, ActiveCharacterTag>())
            {
                input.ValueRW.MoveInput = moveInput;
                input.ValueRW.LookInput = lookInput;
            }

            // Управление курсором: если кликнули - прячем, если Esc - показываем
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}