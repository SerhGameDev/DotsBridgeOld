using SimElectric;
using SimElectric.Interaction;
using Unity.Entities;

namespace DotsBridge
{  
    public static partial class EntityBridge
    {
        /// <summary>
        /// Вызывает нажатие промышленной кнопки.
        /// </summary>
        public static void TriggerButton(this SingleEntity triggerEntity)
        {
            if (!triggerEntity.HasComponent<PushButtonTrigger>()) return;

            var trigger = triggerEntity.GetComponent<PushButtonTrigger>();
            if (trigger.TargetButton == Entity.Null) return;

            var buttonEntity = new SingleEntity(trigger.TargetButton, triggerEntity.Bridge);
            if (!buttonEntity.HasComponent<PushButton>()) return;

            var button = buttonEntity.GetComponent<PushButton>();
            
            button.IsPressed = true;
            button.CurrentTimer = button.AutoReleaseDelay;
            
            buttonEntity.SetComponent(button);
        }
    }
}