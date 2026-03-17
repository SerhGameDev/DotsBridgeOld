using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDE
{
    public class WorkArea
    {
        public string FileId { get; }
        public string Name { get; private set; }
        
        // Хранилище всех нод в этой рабочей области
        public List<Node> Nodes { get; } = new List<Node>();

        public WorkArea(string fileId, string name)
        {
            FileId = fileId;
            Name = name;
            
            // Генерируем тестовую ноду при создании файла (потом уберем)
            string nodeId = Guid.NewGuid().ToString();
            Nodes.Add(new Node(nodeId, $"{name} Main Node", new Vector2(100, 100)));
        }

        public void Rename(string newName) => Name = newName;

        public void Open()
        {
            Debug.Log($"[WorkArea] Opening: {Name} with {Nodes.Count} nodes.");
        }
    }
}