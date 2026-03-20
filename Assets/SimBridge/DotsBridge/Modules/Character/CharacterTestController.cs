using UnityEngine;
using DotsBridge.Character;
using Unity.Collections;

namespace DotsBridge.Test
{
    public class CharacterTestController : MonoBehaviour
    {
        [SerializeField] private string _characterPrefabName = "PlayerCharacter";
        [SerializeField] private string _existingCharacterId = "MainHero_1";

        private SingleEntity _currentCharacter;

        void Update()
        {
            // --- 1. СПАВН И ПЕРЕХВАТ УПРАВЛЕНИЯ ---
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                // Если кем-то уже управляем - отпускаем
                ReleaseCurrentCharacter();

                using var spawnedBatch = EntityBridge.InCurrentWorld()
                    .BeginSpawn(_characterPrefabName)
                    .SetId(_existingCharacterId)
                    .Spawn();

                if (spawnedBatch.Count > 0)
                {
                    _currentCharacter = new SingleEntity(spawnedBatch.Entities[0], EntityBridge.InCurrentWorld());
                    _currentCharacter.Possess();
                }
            }

            // --- 2. ПОИСК СУЩЕСТВУЮЩЕГО ПЕРСОНАЖА ПО ID И ПЕРЕХВАТ ---
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ReleaseCurrentCharacter();

                using var foundBatch = EntityBridge.InCurrentWorld().FindById(_existingCharacterId);
                
                if (foundBatch.Count > 0)
                {
                    _currentCharacter = new SingleEntity(foundBatch.Entities[0], EntityBridge.InCurrentWorld());
                    _currentCharacter.Possess();
                }
                else
                {
                    Debug.LogWarning($"[CharacterTest] Персонаж с ID {_existingCharacterId} не найден!");
                }
            }

            // --- 3. СНЯТИЕ УПРАВЛЕНИЯ (СВОБОДНАЯ КАМЕРА/КУРСОР) ---
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ReleaseCurrentCharacter();
                
                // Возвращаем курсор
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void ReleaseCurrentCharacter()
        {
            if (_currentCharacter.Entity != Unity.Entities.Entity.Null)
            {
                _currentCharacter.Unpossess();
                _currentCharacter = default; // Сбрасываем ссылку
            }
        }
    }
}