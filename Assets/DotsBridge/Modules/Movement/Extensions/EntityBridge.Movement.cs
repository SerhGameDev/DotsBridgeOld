using DotsBridge.Modules.Movement;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        public static ListEntity Move(this ListEntity batch, Vector3 direction) 
            => batch.TrySetComponent(new MoveTransformDirection { Value = direction });
        public static DotsCommand Move(this DotsCommand command, Vector3 direction)
            => command.Do(batch => batch.Move(direction));

        public static ListEntity Move(this ListEntity batch, Vector3 direction, float speed = 5f) 
            => Move(batch, direction).TrySetComponent(new MoveTransformSpeed { Value = speed });

        public static DotsCommand Move(this DotsCommand command, Vector3 direction, float speed = 5f)
            => command.Do(batch => batch.Move(direction, speed));

        public static ListEntity StartMove(this ListEntity batch) 
            => batch.SetEnabled<IsTransformMoving>(true);
        public static DotsCommand StartMove(this DotsCommand command)
            => command.Do(batch => batch.StartMove());

        public static ListEntity StopMove(this ListEntity batch) 
            => batch.SetEnabled<IsTransformMoving>(false);
        public static DotsCommand Stop(this DotsCommand command)
            => command.Do(batch => batch.StopMove());

    }
}