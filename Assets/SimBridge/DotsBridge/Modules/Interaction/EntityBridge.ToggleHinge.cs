using Unity.Entities;
using DotsBridge.Interaction;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Устанавливает целевое состояние для любого механизма (дверь, кнопка, рубильник).
        /// Значение должно быть от 0 (закрыто/выкл) до 1 (открыто/вкл).
        /// </summary>
        public static void SetHingeTarget(this SingleEntity entity, float targetState)
        {
            if (!entity.HasComponent<HingeState>()) return;

            var hinge = entity.GetComponent<HingeState>();
            // Ограничиваем значение жестко от 0 до 1
            hinge.TargetState = Unity.Mathematics.math.clamp(targetState, 0f, 1f);
            entity.SetComponent(hinge);
        }     
        /// <summary>
        /// Устанавливает целевое состояние для любого механизма (дверь, кнопка, рубильник).
        /// Значение должно быть от 0 (закрыто/выкл) до 1 (открыто/вкл).
        /// </summary>
        public static void SetHingeTarget(this ListEntity batch, float targetState)
        {
            SetHingeTarget(batch.First(), targetState);
        }

        /// <summary>
        /// Автоматически переключает состояние между 0 и 1.
        /// </summary>
        public static void ToggleHinge(this SingleEntity entity)
        {
            if (!entity.HasComponent<HingeState>()) return;

            var hinge = entity.GetComponent<HingeState>();
            hinge.TargetState = hinge.TargetState > 0.5f ? 0f : 1f;
            entity.SetComponent(hinge);
        }   
        /// <summary>
        /// Автоматически переключает состояние между 0 и 1.
        /// </summary>
        public static void ToggleHinge(this ListEntity list)
        {
            if (!list.HasComponent<HingeState>()) return;

            for (int i = 0; i < list.Count; i++)
            {
                var entity = list.Entities[i].ToSingleEntity(list.Word);
                var hinge = entity.GetComponent<HingeState>();
                hinge.TargetState = hinge.TargetState > 0.5f ? 0f : 1f;
                entity.SetComponent(hinge);
            }
        }
    }
}