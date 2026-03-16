namespace UIBridge
{
    // === 2. Позиционирование и Макеты ===

    // ViewPosition знает, КАК и КУДА привязать контейнер (анкоры, отступы, родительский трансформ)
    public abstract class ViewPosition
    {
        public abstract void ApplyTo(ViewContainer container);
    }
}