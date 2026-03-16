namespace UIBridge
{
    // === 4. Реализация конкретных окон ===

    public class FixedPanel : ViewContainer
    {
        public const string PrefabId = "UIFixedPanel";

        public override void Add(View view)
        {
            // Логика добавления дочернего объекта (например, LayoutGroup)
            view.transform.SetParent(this.transform, false);
        }

        public override void Remove(View view) { /* ... */ }
        public override void Clear() { /* ... */ }
    }
}