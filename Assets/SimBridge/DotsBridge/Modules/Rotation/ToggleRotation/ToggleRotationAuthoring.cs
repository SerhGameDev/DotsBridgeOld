using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsBridge
{
    public class ToggleRotationAuthoring : MonoBehaviour
    {
        public GameObject targetTransform; // Объект-ось, внутри которого лежат лопасти
        public bool isOn = true;
        public float speedDegrees = 180f;
        public Vector3 axis = Vector3.forward;

        class Baker : Baker<ToggleRotationAuthoring>
        {
            public override void Bake(ToggleRotationAuthoring authoring)
            {
                var targetGo = authoring.targetTransform != null ? authoring.targetTransform : authoring.gameObject;
                
                // 1. Делаем саму ось динамической
                var targetEntity = GetEntity(targetGo, TransformUsageFlags.Dynamic);

                // 2. МАГИЯ ЗДЕСЬ: Просим Unity запечь всех детей (лопасти) как динамические.
                // Встроенный TransformBakingSystem сам расставит компоненты Parent 
                // и математически верно вычислит LocalTransform без смещений орбиты!
                foreach (var child in targetGo.GetComponentsInChildren<Transform>(true))
                {
                    GetEntity(child.gameObject, TransformUsageFlags.Dynamic);
                }

                // 3. Создаем контроллер
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ToggleRotation
                {
                    Target = targetEntity,
                    IsOn = authoring.isOn,
                    Speed = math.radians(authoring.speedDegrees),
                    Axis = math.normalize(authoring.axis)
                });
            }
        }
    }
}