using Unity.Entities;

namespace SimVent.Components
{
    public struct RoomComponent : IComponentData
    {
        public Entity SupplyDuct;         // Ссылка на приточный воздуховод (П1)
                                          // В будущем сюда можно добавить Entity ExhaustDuct (В1) для расчета баланса давления

        public float CurrentTemperature;  // Фактическая температура в комнате (°C)
        public float Volume;              // Объем комнаты (м³) - влияет на скорость нагрева
        public float HeatLossFactor;      // Коэффициент потерь тепла через стены
        public float TargetStreetTemp;    // Температура на улице (куда уходит тепло)
    }
}