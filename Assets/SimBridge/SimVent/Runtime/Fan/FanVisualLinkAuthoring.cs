using DotsBridge;
using Unity.Entities;
using UnityEngine;

namespace SimVent
{
    [RequireComponent(typeof(ToggleRotationAuthoring))] // Защита от ошибок: требует контроллер вращения
    public class FanVisualLinkAuthoring : MonoBehaviour
    {
        [Tooltip("Объект, на котором висит логика вентилятора (FanComponent). Если пусто - берет с себя.")]
        public GameObject logicalFan;
        
        [Tooltip("Множитель: переводит CurrentSpeed в радианы в секунду для визуала")]
        public float speedMultiplier = 10f;

        class Baker : Baker<FanVisualLinkAuthoring>
        {
            public override void Bake(FanVisualLinkAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                
                // Находим логическую сущность
                var logicalEntity = authoring.logicalFan != null 
                    ? GetEntity(authoring.logicalFan, TransformUsageFlags.Dynamic) 
                    : GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new FanVisualLink
                {
                    LogicalFan = logicalEntity,
                    SpeedMultiplier = authoring.speedMultiplier
                });
            }
        }
    }
}