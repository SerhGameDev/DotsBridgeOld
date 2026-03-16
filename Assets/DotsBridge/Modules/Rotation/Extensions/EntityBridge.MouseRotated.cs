using DotsBridge.Modules.Rotation;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        public static ListEntity SetMouseRotationSensitivity(this ListEntity batch, float sensitivity)
            => batch.TrySetComponent(new MouseRotationConfig { Sensitivity = sensitivity });

        public static bool CanBeMouseRotated(this SingleEntity entity)
            => entity.HasComponent<MouseRotationConfig>();

        public static ListEntity EnableMouseRotation(this ListEntity batch)
            => batch.SetEnabled<CanBeMouseRotated>(true);
        public static SingleEntity EnableMouseRotation(this SingleEntity entity)
            => entity.SetEnabled<CanBeMouseRotated>(true);


        public static ListEntity DisableMouseRotation(this ListEntity batch)
            => batch.SetEnabled<CanBeMouseRotated>(false);
        public static SingleEntity DisableMouseRotation(this SingleEntity entity)
            => entity.SetEnabled<CanBeMouseRotated>(false);
        public static SingleEntity SetInertia(this SingleEntity batch, bool use, float friction = 0.95f)
        {
            if (batch.HasComponent<MouseRotationConfig>())
            {
                var mouseRotationConfig = batch.GetComponent<MouseRotationConfig>();
                mouseRotationConfig.UseInertia = use;
                mouseRotationConfig.Friction = friction;
                batch.SetComponent(mouseRotationConfig);
            }
            return batch;
        }
        public static ListEntity SetInertia(this ListEntity batch, bool use, float friction = 0.95f)
        {
            for (int i = 0; i < batch.Count; i++)
            {
                if (batch.Word.Manager.HasComponent<MouseRotationConfig>(batch.Entities[i]))
                {
                    var cfg = batch.Word.Manager.GetComponentData<MouseRotationConfig>(batch.Entities[i]);
                    cfg.UseInertia = use;
                    cfg.Friction = friction;
                    batch.Word.Manager.SetComponentData(batch.Entities[i], cfg);
                }
            }
            return batch;
        }
    }
}