namespace IDE
{
    public interface IHierarchyItemData
    {
        string Id { get; }
        string Name { get; }
        int ExecutionOrder { get; }
        bool IsFolder { get; }
        string ParentId { get; } 
    }
}