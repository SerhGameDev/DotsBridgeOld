using UnityEngine;
using Unity.Mathematics;
using DotsBridge.Character;

namespace DotsBridge.Character
{
    /// <summary>
    /// Скрипт-мост. Берет данные из твоей кастомной библиотеки ввода 
    /// и передает их конкретной ECS-сущности.
    /// </summary>
    public class LocalCharacterInputSync : MonoBehaviour
    {
        // Сущность, в которую мы сейчас "вливаем" инпут
        public SingleEntity ControlledEntity;

        void Update()
        {
            // Если никем не управляем — ничего не делаем
            if (ControlledEntity.Entity == Unity.Entities.Entity.Null) return;

            // ==========================================
            // ЗДЕСЬ ТЫ ПОДКЛЮЧАЕШЬ СВОЮ БИБЛИОТЕКУ ВВОДА
            // ==========================================
            
            // Пример: заменяешь Input.GetAxisRaw на MyAwesomeInput.GetAxis(...)
            float moveX = Input.GetAxisRaw("Horizontal"); 
            float moveY = Input.GetAxisRaw("Vertical");
            
            float lookX = Input.GetAxisRaw("Mouse X");
            float lookY = Input.GetAxisRaw("Mouse Y");

            float2 moveInput = new float2(moveX, moveY);
            
            // Нормализация вектора движения (чтобы по диагонали не бегать быстрее)
            if (math.lengthsq(moveInput) > 1f)
            {
                moveInput = math.normalize(moveInput);
            }

            // Передаем собранные данные в DOTS-сущность
            ControlledEntity.SetComponent(new CharacterControlInput
            {
                MoveInput = moveInput,
                LookInput = new float2(lookX, lookY)
            });
        }
    }
}