using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace SimElectric.Interaction
{
    [RequireComponent(typeof(WireAuthoring))] // Кнопка всегда размыкает/замыкает провод
    public class PushButtonAuthoring : MonoBehaviour
    {
        [Title("Button Settings")]
        [Tooltip("Если true, кнопка сама отожмется через заданное время (имитация пружины)")]
        public bool isMomentary = true;
        
        [Tooltip("Время удержания контакта в секундах (пока работает пружина)")]
        [ShowIf("isMomentary")]
        public float autoReleaseDelay = 0.5f;

        public class Baker : Baker<PushButtonAuthoring>
        {
            public override void Bake(PushButtonAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new PushButton
                {
                    IsMomentary = authoring.isMomentary,
                    IsPressed = false,
                    AutoReleaseDelay = authoring.autoReleaseDelay,
                    CurrentTimer = 0f
                });

                // Чтобы по ней можно было кликнуть
                AddComponent(entity, new PushButtonTrigger { TargetButton = entity });
            }
        }
    }
    // Компонент самой кнопки

    // Триггер для клика (вешается туда же, куда и коллайдер)
}