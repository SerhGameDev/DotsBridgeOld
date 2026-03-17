namespace IDE
{
    public class HierarchyItemData : IHierarchyItemData
    {
        public string Id { get; }
        public string Name { get; set; }
        public int ExecutionOrder { get; set; }
        public bool IsFolder { get; }
        public string ParentId { get; set; }

        public HierarchyItemData(string id, string name, bool isFolder, string parentId = null, int executionOrder = 0)
        {
            Id = id;
            Name = name;
            IsFolder = isFolder;
            ParentId = parentId;
            ExecutionOrder = executionOrder;
        }
    }
}