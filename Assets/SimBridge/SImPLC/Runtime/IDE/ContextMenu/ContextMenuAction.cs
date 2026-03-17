using System;

namespace IDE
{
    public class ContextMenuAction
    {
        public string Name { get; }
        public Action Command { get; }

        public ContextMenuAction(string name, Action command)
        {
            Name = name;
            Command = command;
        }
    }
}