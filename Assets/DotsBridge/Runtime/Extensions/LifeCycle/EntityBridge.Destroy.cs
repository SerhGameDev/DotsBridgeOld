using System.Runtime.CompilerServices;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        /// <summary>
        /// Помечает сущности на удаление.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DotsCommand Destroy(this DotsCommand cmd)
        {
            return cmd.Do(batch =>
            {
                if (batch.Entities.IsCreated && batch.Entities.Length > 0)
                {
                    batch.Manager.AddComponent<DestroyTag>(batch.Entities.AsArray());
                }
            });
        }
    }
}