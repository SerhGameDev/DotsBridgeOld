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
        public static EntityBatch Move(this EntityBatch batch, Vector3 direction) 
            => batch.SetData(new MoveTransformDirection { Value = direction });
        public static DotsCommand Move(this DotsCommand command, Vector3 direction)
            => command.Do(batch => batch.Move(direction));

        public static EntityBatch Move(this EntityBatch batch, Vector3 direction, float speed = 5f) 
            => Move(batch, direction).SetData(new MoveTransformSpeed { Value = speed });

        public static DotsCommand Move(this DotsCommand command, Vector3 direction, float speed = 5f)
            => command.Do(batch => batch.Move(direction, speed));

        public static EntityBatch StartMove(this EntityBatch batch) 
            => batch.SetEnabled<IsTransformMoving>(true);
        public static DotsCommand StartMove(this DotsCommand command)
            => command.Do(batch => batch.StartMove());

        public static EntityBatch StopMove(this EntityBatch batch) 
            => batch.SetEnabled<IsTransformMoving>(false);
        public static DotsCommand Stop(this DotsCommand command)
            => command.Do(batch => batch.StopMove());

    }
}