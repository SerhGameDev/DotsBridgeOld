using Unity.Burst;
using Unity.Entities;

namespace SimElectric
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    // ВАЖНО: Работает после расчета токов!
    [UpdateAfter(typeof(CircuitSolverSystem))]
    [BurstCompile]
    public partial struct CircuitBreakerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationControl>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var control = SystemAPI.GetSingleton<SimulationControl>();
            if (!control.IsRunning && !control.StepNextFrame) return;

            // Запускаем джоб по всем сущностям, у которых есть и CircuitBreaker, и Wire
            var job = new CircuitBreakerJob();
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct CircuitBreakerJob : IJobEntity
        {
            // Берем Wire по ссылке (ref), чтобы размыкать цепь, если нужно
            private void Execute(ref CircuitBreaker breaker, ref Wire wire)
            {
                // 1. Если автомат уже выбит, он жестко держит цепь разомкнутой
                if (breaker.IsTripped)
                {
                    wire.IsConducting = false;
                    return;
                }

                // 2. Защита от ручного переключения в Инспекторе
                // Если автомат взведен (не выбит), убеждаемся, что провод проводит ток
                if (!breaker.IsTripped && !wire.IsConducting)
                {
                    wire.IsConducting = true;
                }

                // 3. ПРОВЕРКА НА ПЕРЕГРУЗКУ ИЛИ КЗ
                if (wire.CurrentFlow > breaker.RatedCurrent)
                {
                    // Автомат выбивает!
                    breaker.IsTripped = true;
                    wire.IsConducting = false;

                    // Обнуляем ток для визуальной чистоты данных в этом кадре
                    wire.CurrentFlow = 0f;
                }
            }
        }
    }
}