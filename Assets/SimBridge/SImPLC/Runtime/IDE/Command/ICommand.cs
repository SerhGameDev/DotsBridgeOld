namespace IDE
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}