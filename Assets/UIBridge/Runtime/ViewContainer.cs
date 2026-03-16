namespace UIBridge
{
    // ViewContainer - это тоже View, но с логикой управления дочерними элементами
    public abstract class ViewContainer : View
    {
        public abstract void Add(View view);
        public abstract void Remove(View view);
        public abstract void Clear();
    }
}