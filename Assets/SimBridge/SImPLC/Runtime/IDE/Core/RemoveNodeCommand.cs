namespace IDE
{
    public class RemoveNodeCommand : ICommand
    {
        private readonly WorkArea _workArea;
        private readonly Node _node;

        public RemoveNodeCommand(WorkArea workArea, Node node)
        {
            _workArea = workArea;
            _node = node;
        }

        // Прямое действие - удаляем
        public void Execute() => _workArea.RemoveNode(_node.Id);

        // Отмена - возвращаем ту самую ноду
        public void Undo() => _workArea.AddNode(_node);
    }
}