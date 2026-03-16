using Unity.Entities;
using UnityEngine;

namespace SimElectric
{
    public class SimulationControlAuthoring : MonoBehaviour
    {
        public bool isRunning = true; 
        public bool enableWireHighlight = true; 

        public class Baker : Baker<SimulationControlAuthoring>
        {
            public override void Bake(SimulationControlAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SimulationControl
                {
                    IsRunning = authoring.isRunning,
                    StepNextFrame = false
                });

                if (authoring.enableWireHighlight)
                {
                    AddComponent<EnableWireHighlightTag>(entity);
                }
            }
        }
    }
}