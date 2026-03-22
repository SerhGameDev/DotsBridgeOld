using Unity.Entities;
using UnityEngine;

namespace DotsBridge.Interaction
{
    public class HingeTriggerAuthoring : MonoBehaviour
    {
        [Tooltip("Укажите объект на сцене, на котором висит HingeAuthoring")]
        public HingeAuthoring TargetHinge;

        public class TriggerBaker : Baker<HingeTriggerAuthoring>
        {
            public override void Bake(HingeTriggerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                // Автоматически добавляем ваш тег для системы наведения луча!
                AddComponent<InteractableTag>(entity);
                
                AddComponent(entity, new HingeTrigger
                {
                    TargetHinge = GetEntity(authoring.TargetHinge, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}