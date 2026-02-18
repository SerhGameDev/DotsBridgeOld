using DotsBridge.Timeline;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DotsBridge.Timeline
{
    public struct RelativeGroupBuilder
    {
        private EntityManager _manager;
        private NativeArray<Entity> _entities;

        private NativeList<RelativeMoveClip> _recipe;
        private float _cursorTime;
        private float3 _virtualOffset; 

        public RelativeGroupBuilder(EntityManager manager, NativeArray<Entity> entities, Allocator allocator)
        {
            _manager = manager;
            _entities = entities;
            _recipe = new NativeList<RelativeMoveClip>(16, allocator);
            _cursorTime = 0;
            _virtualOffset = float3.zero;
        }

        public RelativeGroupBuilder Move(Vector3 offsetDirection, float duration, Ease ease = Ease.OutQuad)
        {
            float3 start = _virtualOffset;
            float3 end = _virtualOffset + (float3)offsetDirection;

            _recipe.Add(new RelativeMoveClip
            {
                StartTime = _cursorTime,
                Duration = duration,
                StartOffset = start,
                EndOffset = end,
                Easing = ease
            });

            _virtualOffset = end;
            _cursorTime += duration;
            return this;
        }

        public RelativeGroupBuilder Wait(float duration)
        {
            _cursorTime += duration;
            return this;
        }

        public void Build()
        {
            if (_entities.Length == 0)
            {
                _recipe.Dispose();
                return;
            }

            _manager.AddComponent<TimelineState>(_entities);
            _manager.AddComponent<RelativeMoveClip>(_entities);
            _manager.AddComponent<SequenceOrigin>(_entities);
            _manager.AddComponent<LocalTransform>(_entities);

            var resetState = new TimelineState { CurrentTime = 0, CurrentClipIndex = 0, PlaybackSpeed = 1, IsPlaying = true };
            var resetOrigin = new SequenceOrigin { IsCaptured = false, Value = float3.zero };

            for (int i = 0; i < _entities.Length; i++)
            {
                Entity e = _entities[i];

                _manager.SetComponentData(e, resetState);
                _manager.SetComponentData(e, resetOrigin);
                _manager.SetComponentEnabled<TimelineState>(e, true);

                DynamicBuffer<RelativeMoveClip> buffer = _manager.GetBuffer<RelativeMoveClip>(e);
                buffer.Clear();
                buffer.CopyFrom(_recipe);
            }

            _recipe.Dispose();
        }
    }
}