using SimElectric;
using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace SimElectric
{
    [RequireComponent(typeof(ElectricalNodeAuthoring))]
    public class LightbulbAuthoring : MonoBehaviour
    {
        [Title("Lightbulb Settings")]
        public float thresholdVoltage = 12f;
        public Color onColor = Color.yellow;
        public Color offColor = Color.black;

        public class Baker : Baker<LightbulbAuthoring>
        {
            public override void Bake(LightbulbAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Lightbulb
                {
                    ThresholdVoltage = authoring.thresholdVoltage,
                    OnColor = new float4(authoring.onColor.r, authoring.onColor.g, authoring.onColor.b, authoring.onColor.a),
                    OffColor = new float4(authoring.offColor.r, authoring.offColor.g, authoring.offColor.b, authoring.offColor.a)
                });

                AddComponent(entity, new URPMaterialPropertyBaseColor { Value = new float4(0, 0, 0, 1) });
            }
        }
    }
}