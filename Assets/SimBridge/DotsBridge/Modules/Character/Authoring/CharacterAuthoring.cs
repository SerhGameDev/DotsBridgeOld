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
        public float EyeHeight = 1.6f; // Высота глаз взрослого человека

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
                AddComponent<CharacterViewState>(entity);
                
                AddComponent(entity, new CharacterSettings
                {
                    MoveSpeed = authoring.MoveSpeed,
                    LookSpeed = authoring.LookSpeed,
                    StepHeight = authoring.StepHeight,
                    CharacterRadius = authoring.CharacterRadius,
                    Gravity = authoring.Gravity,
                    EyeHeight = authoring.EyeHeight 
                });

                AddComponent<ActiveCharacterTag>(entity);
                SetComponentEnabled<ActiveCharacterTag>(entity, authoring.StartAsActivePlayer);
            }
        }
    }
}