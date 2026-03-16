using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimPLS
{
    public class NodeManager
    {
        public List<Node> AllNodes { get; } = new List<Node>();

        private VisualElement workspace;
        private VisualTreeAsset nodeUXML, inPortUXML, outPortUXML;
    
        // Зависимости
        private SelectionManager selectionManager;
        private ConnectionManager connectionManager;

        public NodeManager(VisualElement workspace, VisualTreeAsset n, VisualTreeAsset i, VisualTreeAsset o, 
            SelectionManager selMgr, ConnectionManager connMgr)
        {
            this.workspace = workspace;
            nodeUXML = n; inPortUXML = i; outPortUXML = o;
            selectionManager = selMgr;
            connectionManager = connMgr;
        }

        public void CreateNode(Type nodeType, Vector2 position)
        {
            Node newNode = (Node)Activator.CreateInstance(nodeType, nodeUXML, inPortUXML, outPortUXML);
        
            float snappedX = Mathf.Round(position.x / 20f) * 20f;
            float snappedY = Mathf.Round(position.y / 20f) * 20f;
            newNode.SetPosition(snappedX, snappedY);

            // Подписываемся на события ноды и прокидываем их в соответствующие менеджеры
            newNode.OnNodeSelected += selectionManager.Select;
            newNode.OnPortDragStarted += connectionManager.StartConnection;

            workspace.Add(newNode);
            newNode.SetupPorts();
            AllNodes.Add(newNode);
        }

        public void DeleteNode(Node node)
        {
            // Сначала удаляем все провода, связанные с нодой
            connectionManager.RemoveConnectionsForNode(node);

            node.OnNodeSelected -= selectionManager.Select;
            node.OnPortDragStarted -= connectionManager.StartConnection;
        
            AllNodes.Remove(node);
            workspace.Remove(node);
        
            if (selectionManager.SelectedNode == node) selectionManager.Deselect();
        }
    }
}