namespace SimPLS
{
    public class SelectionManager
    {
        public Node SelectedNode { get; private set; }

        public void Select(Node node)
        {
            Deselect();
            SelectedNode = node;
            SelectedNode.SetSelectedStyle(true);
        }

        public void Deselect()
        {
            if (SelectedNode != null)
            {
                SelectedNode.SetSelectedStyle(false);
                SelectedNode = null;
            }
        }
    }
}