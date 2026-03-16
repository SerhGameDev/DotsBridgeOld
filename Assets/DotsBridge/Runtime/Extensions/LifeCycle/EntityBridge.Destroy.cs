using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Помечает сущности на удаление (будут уничтожены в конце кадра системным сборщиком).
        /// </summary>
        public static ListEntity Destroy(this ListEntity batch)
        {
            if (batch.Entities.IsCreated && batch.Entities.Length > 0)
            {
                batch.Manager.AddComponent<DestroyTag>(batch.Entities.AsArray());
            }
            return batch;
        }
        public static DotsCommand Destroy(this DotsCommand cmd) => cmd.Do(b => b.Destroy());
    }
}