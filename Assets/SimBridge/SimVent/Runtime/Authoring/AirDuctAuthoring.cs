using SimVent.Components;
using Unity.Entities;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SimVent.Authoring
{
    public class AirDuctAuthoring : MonoBehaviour
    {
        [Title("Топология сети (Откуда -> Куда)")]
        [Required] public AirNodeAuthoring SourceNode;
        [Required] public AirNodeAuthoring TargetNode;

        class Baker : Baker<AirDuctAuthoring>
        {
            public override void Bake(AirDuctAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new AirDuctComponent
                {
                    SourceNode = GetEntity(authoring.SourceNode, TransformUsageFlags.None),
                    TargetNode = GetEntity(authoring.TargetNode, TransformUsageFlags.None),
                    CurrentFlowRate = 0f,
                    AirTemperature = 0f
                }); 
                AddComponent(entity, new DuctStateComponent { TotalResistance = 0.5f });
            }
        }
    }
}