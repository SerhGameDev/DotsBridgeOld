using System;

namespace UIBridge
{
    // === 3. Логика (ViewModel) ===

    public abstract class ViewModel : IDisposable
    {
        // Вызываем при создании для сборки интерфейса
        public abstract void Build();

        // Обязательно реализуем IDisposable для отписки от событий и возврата UI в пул
        public abstract void Dispose();
    }
}