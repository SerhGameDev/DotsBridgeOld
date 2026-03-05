using Unity.Entities;
using UnityEngine;

namespace DotsBridge.Modules.Sync
{
    /// <summary>
    /// Managed-компонент для хранения прямой ссылки на Transform объекта.
    /// </summary>
    public class LinkedTransform : IComponentData
    {
        public Transform Transform;
    }
}