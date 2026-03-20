using Unity.Entities;
using UnityEngine;

namespace DotsBridge.Character
{
    [DisallowMultipleComponent]
    public class CharacterAuthoring : MonoBehaviour
    {
        [Header("Movement")]
        public float MoveSpeed = 5f;
        public float LookSpeed = 2f;
        
        [Header("Physics & Collisions")]
        public float StepHeight = 0.3f;
        public float CharacterRadius = 0.4f;
        public float Gravity = -15f;

        [Header("State")]
        public bool StartAsActivePlayer = false;

        public class CharacterBaker : Baker<CharacterAuthoring>
        {
            public override void Bake(CharacterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<CharacterTag>(entity);
                AddComponent<CharacterControlInput>(entity);
                AddComponent<CharacterVelocity>(entity);
                
                AddComponent(entity, new CharacterSettings
                {
                    MoveSpeed = authoring.MoveSpeed,
                    LookSpeed = authoring.LookSpeed,
                    StepHeight = authoring.StepHeight,
                    CharacterRadius = authoring.CharacterRadius,
                    Gravity = authoring.Gravity
                });

                // Добавляем тег активности, но по умолчанию выключаем его, 
                // если не сказано иное (для системы Possess)
                AddComponent<ActiveCharacterTag>(entity);
                SetComponentEnabled<ActiveCharacterTag>(entity, authoring.StartAsActivePlayer);
            }
        }
    }
}