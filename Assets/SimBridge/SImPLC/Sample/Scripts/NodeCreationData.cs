using System;

namespace SimPLS
{
    public struct NodeCreationData
    {
        public string MenuName;  // Имя в поиске (например "AND Gate")
        public string Category;  // Категория (например "Logic" или "Timers")
        public Type NodeType;    // Сам класс ноды (например typeof(NodeAND))

        public NodeCreationData(string name, string category, Type type)
        {
            MenuName = name;
            Category = category;
            NodeType = type;
        }
    }
}