using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge.Interaction
{
    public class HingeAuthoring : MonoBehaviour
    {
        [Tooltip("Углы поворота в закрытом состоянии (State = 0)")]
        public Vector3 ClosedEulerAngles = Vector3.zero;
        
        [Tooltip("Углы поворота в открытом состоянии (State = 1)")]
        public Vector3 OpenEulerAngles = new Vector3(0, 90f, 0);
        
        [Tooltip("Скорость анимации (единиц State в секунду)")]
        public float Speed = 5f;

        [Tooltip("Начальное состояние (например, сразу открыто)")]
        [Range(0f, 1f)] public float InitialState = 0f;

        public class HingeBaker : Baker<HingeAuthoring>
        {
            public override void Bake(HingeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                // Переводим градусы в радианы, а затем в кватернионы для DOTS
                quaternion closedRot = quaternion.Euler(math.radians(authoring.ClosedEulerAngles));
                quaternion openRot = quaternion.Euler(math.radians(authoring.OpenEulerAngles));

                AddComponent(entity, new HingeState
                {
                    CurrentState = authoring.InitialState,
                    TargetState = authoring.InitialState,
                    Speed = authoring.Speed,
                    ClosedRotation = closedRot,
                    OpenRotation = openRot
                });
            }
        }
    }
}