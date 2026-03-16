using Unity.Entities;

namespace SimVent.Authoring
{
    // Вспомогательный компонент для хранения ссылок
    public struct DuctEquipmentRefs : IComponentData
    {
        public Entity Fan;
        public Entity Damper;
        public Entity Heater;
    }
}