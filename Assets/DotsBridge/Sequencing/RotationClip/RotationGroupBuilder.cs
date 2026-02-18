
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge.Timeline
{
    public struct RotationGroupBuilder
    {
        private EntityManager _manager;
        private NativeArray<Entity> _entities;

        private NativeList<RotationClip> _recipe;
        private float _cursorTime;

        private float3 _virtualEulerOffset;

        private int _loops;

        public RotationGroupBuilder(EntityManager manager, NativeArray<Entity> entities, Allocator allocator)
        {
            _manager = manager;
            _entities = entities;
            _recipe = new NativeList<RotationClip>(16, allocator);
            _cursorTime = 0;
            _virtualEulerOffset = float3.zero;
            _loops = 0; 
        }

        public RotationGroupBuilder Rotate(Vector3 eulerAngles, float duration, Ease ease = Ease.OutQuad)
        {
            float3 step = eulerAngles;
            float3 start = _virtualEulerOffset;
            float3 end = _virtualEulerOffset + step;

            _recipe.Add(new RotationClip
            {
                StartTime = _cursorTime,
                Duration = duration,
                StartEuler = start,
                EndEuler = end,
                Easing = ease
            });

            _virtualEulerOffset = end;
            _cursorTime += duration;
            return this;
        }

        public RotationGroupBuilder Wait(float duration)
        {
            _cursorTime += duration;
            return this;
        }

        public RotationGroupBuilder SetLoops(int loops)
        {
            _loops = loops;
            return this;
        }

        public void Build()
        {
            if (_entities.Length == 0)
            {
                if (_recipe.IsCreated) _recipe.Dispose();
                return;
            }

            _manager.AddComponent<RotationTimelineState>(_entities);
            _manager.AddComponent<RotationClip>(_entities);
            _manager.AddComponent<RotationOrigin>(_entities);

            var state = new RotationTimelineState
            {
                CurrentTime = 0,
                CurrentClipIndex = 0,
                PlaybackSpeed = 1,
                IsPlaying = true,
                Loops = _loops 
            };

            var resetOrigin = new RotationOrigin { IsCaptured = false, Value = quaternion.identity };

            for (int i = 0; i < _entities.Length; i++)
            {
                Entity e = _entities[i];

                _manager.SetComponentData(e, state);
                _manager.SetComponentData(e, resetOrigin);
                _manager.SetComponentEnabled<RotationTimelineState>(e, true);

                DynamicBuffer<RotationClip> buffer = _manager.GetBuffer<RotationClip>(e);
                buffer.Clear();

                buffer.CopyFrom(_recipe.AsArray());
            }

            if (_recipe.IsCreated) 
                _recipe.Dispose();
        }
    }
}