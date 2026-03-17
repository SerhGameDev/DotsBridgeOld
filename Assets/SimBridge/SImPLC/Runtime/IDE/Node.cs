using UnityEngine;

namespace IDE
{
    public class Node
    {
        public string Id { get; }
        public string Name { get; set; }
        public Vector2 Position { get; set; }

        public Node(string id, string name, Vector2 position)
        {
            Id = id;
            Name = name;
            Position = position;
        }
    }
}