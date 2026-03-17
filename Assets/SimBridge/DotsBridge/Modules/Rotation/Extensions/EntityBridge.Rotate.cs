using DotsBridge.Modules.Rotation;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        // Настройка оси с выбором пространства
        public static ListEntity Rotate(this ListEntity batch, Vector3 axis, Space space = Space.Self)
        {
            // Безопасная нормализация прямо здесь
            float3 normAxis = math.lengthsq(axis) > 0.0001f ? math.normalize((float3)axis) : float3.zero;

            return batch.TrySetComponent(new RotateTransformAxis
            {
                Value = normAxis,
                IsLocal = space == Space.Self
            });
        }

        public static DotsCommand Rotate(this DotsCommand command, Vector3 axis, Space space = Space.Self)
            => command.Do(batch => batch.Rotate(axis, space));

        // Настройка оси, скорости и пространства
        public static ListEntity Rotate(this ListEntity batch, Vector3 axis, float speed, Space space = Space.Self)
            => Rotate(batch, axis, space).TrySetComponent(new RotateTransformSpeed { Value = speed });

        public static DotsCommand Rotate(this DotsCommand command, Vector3 axis, float speed, Space space = Space.Self)
            => command.Do(batch => batch.Rotate(axis, speed, space));

        // Старт/Стоп остались без изменений
        public static ListEntity StartRotate(this ListEntity batch) => batch.SetEnabled<IsTransformRotating>(true);
        public static DotsCommand StartRotate(this DotsCommand command) => command.Do(batch => batch.StartRotate());
        public static ListEntity StopRotate(this ListEntity batch) => batch.SetEnabled<IsTransformRotating>(false);
        public static DotsCommand StopRotate(this DotsCommand command) => command.Do(batch => batch.StopRotate());
    }
}