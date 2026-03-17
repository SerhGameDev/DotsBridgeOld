namespace IDE
{
    public class HierarchyItemData : IHierarchyItemData
    {
        public string Id { get; }
        public string Name { get; set; }
        public int ExecutionOrder { get; set; }
        public bool IsFolder { get; }

        public HierarchyItemData(string id, string name, bool isFolder, int executionOrder = 0)
        {
            Id = id;
            Name = name;
            IsFolder = isFolder;
            ExecutionOrder = executionOrder;
        }
    }
}