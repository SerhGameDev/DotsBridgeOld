using DotsBridge.Modules.Rotation;
using System;

namespace DotsBridge
{
    public static partial class EntityBridge
    {

        public static event Action<SingleEntity> OnMouseEnter;
        public static event Action<SingleEntity> OnMouseExit;

        internal static void TriggerMouseEnter(this SingleEntity entity) => OnMouseEnter?.Invoke(entity);
        internal static void TriggerMouseExit(this SingleEntity entity) => OnMouseExit?.Invoke(entity);

        /// <summary>
        /// Быстрая проверка: наведена ли мышь на эту конкретную сущность прямо сейчас?
        /// </summary>
        public static bool IsHovered(this SingleEntity entity)
        {
            if (entity.Entity == Unity.Entities.Entity.Null || ClientBridge.World() == null) return false;
            return ClientBridge.World().Manager.HasComponent<HoveredTag>(entity.Entity);
        }
    }
}