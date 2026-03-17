namespace IDE
{
    public class AddNodeCommand : ICommand
    {
        private readonly WorkArea _workArea;
        private readonly Node _node;

        public AddNodeCommand(WorkArea workArea, Node node)
        {
            _workArea = workArea;
            _node = node;
        }

        // Прямое действие - добавляем
        public void Execute() => _workArea.AddNode(_node);

        // Отмена - удаляем по ID
        public void Undo() => _workArea.RemoveNode(_node.Id);
    }
}