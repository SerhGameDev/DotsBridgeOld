using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDE
{
    public class WorkArea
    {
        public string FileId { get; }
        public string Name { get; private set; }
        
        public List<Node> Nodes { get; } = new List<Node>();
        public CommandHistory History { get; } = new CommandHistory();

        // События для связи с UI
        public event Action<Node> OnNodeAdded;
        public event Action<string> OnNodeRemoved;

        public WorkArea(string fileId, string name)
        {
            FileId = fileId;
            Name = name;
        }

        public void Rename(string newName) => Name = newName;

        public void Open()
        {
            Debug.Log($"[WorkArea] Opening: {Name} with {Nodes.Count} nodes.");
        }


        public void AddNode(Node node)
        {
            if (node == null || Nodes.Exists(n => n.Id == node.Id)) return;
            
            Nodes.Add(node);
            OnNodeAdded?.Invoke(node); // Оповещаем UI, что нужно нарисовать ноду
        }

        public void RemoveNode(string nodeId)
        {
            var node = Nodes.Find(n => n.Id == nodeId);
            if (node != null)
            {
                Nodes.Remove(node);
                OnNodeRemoved?.Invoke(nodeId); // Оповещаем UI, что нужно стереть ноду
            }
        }
    }
}