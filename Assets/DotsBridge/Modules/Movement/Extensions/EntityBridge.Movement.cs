using DotsBridge.Modules.Movement;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge
{
    public static partial class EntityBridge
    {
        public static EntityBatch Move(this EntityBatch batch, Vector3 position)
        {
            batch.SetData(new MoveTransformDirection { Value = position });
            return batch;
        }

        public static DotsCommand Move(this DotsCommand сommand, Vector3 position)
        {
            return сommand.Do(batch => batch.Move(position));
        }

        public static DotsCommand Move(this DotsCommand cmd, Func<float3> positionGetter)
        {
            return cmd.Do(batch => batch.Move(positionGetter.Invoke()));
        }
    }
}