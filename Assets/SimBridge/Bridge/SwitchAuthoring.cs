using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using DotsBridge.Interaction;

namespace SimElectric
{
    [RequireComponent(typeof(WireAuthoring))]
    [RequireComponent(typeof(HingeAuthoring))]
    public class SwitchAuthoring : MonoBehaviour
    {
        [Title("Switch Settings")]
        [Tooltip("Включен ли рубильник по умолчанию")]
        public bool initialIsOn = false;

        public class Baker : Baker<SwitchAuthoring>
        {
            public override void Bake(SwitchAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Добавляем компонент переключателя
                AddComponent(entity, new SwitchComponent
                {
                    IsOn = authoring.initialIsOn
                });

                // Добавляем триггер, чтобы игрок мог кликать прямо по рубильнику
                // TargetHinge указывает на саму себя
                AddComponent(entity, new HingeTrigger
                {
                    TargetHinge = entity 
                });
            }
        }
    }
}