using UnityEngine;
using DotsBridge.Character;
using Unity.Collections;

namespace DotsBridge.Test
{
    [RequireComponent(typeof(LocalCharacterInputSync))]
    public class CharacterTestController : MonoBehaviour
    {
        [SerializeField] private string _characterPrefabName = "PlayerCharacter";
        [SerializeField] private string _existingCharacterId = "MainHero_1";

        private SingleEntity _currentCharacter;
        private LocalCharacterInputSync _inputSync;
        private void Awake()
        {
            _inputSync = GetComponent<LocalCharacterInputSync>();
        }
        private void Update()
        {
            // --- 1. СПАВН И ПЕРЕХВАТ УПРАВЛЕНИЯ ---
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                // Если кем-то уже управляем - отпускаем
                ReleaseCurrentCharacter();

                using var spawnedBatch = ClientBridge.World()
                    .BeginSpawn(_characterPrefabName)
                    .SetPosition(new Vector3(0,10,0) )
                    .SetId(_existingCharacterId)
                    .Spawn();
              

                if (spawnedBatch.Count > 0)
                {
                    _currentCharacter = new SingleEntity(spawnedBatch.Entities[0], ClientBridge.World());
                    _currentCharacter.Possess().AttachFirstPersonCamera(); 
                }
                _inputSync.ControlledEntity = _currentCharacter;
            }

            // --- 2. ПОИСК СУЩЕСТВУЮЩЕГО ПЕРСОНАЖА ПО ID И ПЕРЕХВАТ ---
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ReleaseCurrentCharacter();

                using var foundBatch = ClientBridge.World().FindById(_existingCharacterId);
                
                if (foundBatch.Count > 0)
                {
                    _currentCharacter = new SingleEntity(foundBatch.Entities[0], ClientBridge.World());
                    _currentCharacter.Possess();
                }
                else
                {
                    Debug.LogWarning($"[CharacterTest] Персонаж с ID {_existingCharacterId} не найден!");
                }
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