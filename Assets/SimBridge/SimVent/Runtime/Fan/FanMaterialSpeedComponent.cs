using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

// Обязательно!

namespace SimVent.Components
{
    // Этот атрибут магически связывает компонент с Hybrid Renderer.
    // "_RotationSpeed" должно точно совпадать с именем свойства в Shader Graph (Reference name).
    [MaterialProperty("_RotationSpeed")]
    public struct FanMaterialSpeedComponent : IComponentData
    {
        public float Value;
    }

}