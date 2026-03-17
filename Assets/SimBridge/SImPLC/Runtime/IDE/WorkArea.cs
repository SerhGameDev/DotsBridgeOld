namespace IDE
{
    // Пока это просто контейнер, который в будущем будет хранить граф нод
    public class WorkArea
    {
        public string FileId { get; }
        public string Name { get; private set; }

        public WorkArea(string fileId, string name)
        {
            FileId = fileId;
            Name = name;
        }

        public void Open()
        {
            UnityEngine.Debug.Log($"[WorkArea] Opening workspace for: {Name} (ID: {FileId})");
            // Здесь в будущем будет логика переключения видимости графа нод
        }
    }
}