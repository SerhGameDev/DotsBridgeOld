using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimPLS
{
    public class ConnectionManager
    {
        public List<Connection> AllConnections { get; } = new List<Connection>();
    
        private VisualElement workspace;
        private Connection activeTempConnection;
        private Port startDragPort;
        private bool isDrawing = false;

        public ConnectionManager(VisualElement workspace)
        {
            this.workspace = workspace;
        
            workspace.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            workspace.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        public void StartConnection(Port port)
        {
            if (port.IsInput)
            {
                if (port.ConnectedPorts.Count > 0)
                {
                    Port connectedOutput = port.ConnectedPorts[0];
                    Connection existingConn = AllConnections.Find(c => c.PortB == port && c.PortA == connectedOutput);
                
                    if (existingConn != null)
                    {
                        port.Disconnect(connectedOutput);
                        AllConnections.Remove(existingConn);
                        activeTempConnection = existingConn;
                        activeTempConnection.DetachPortB();
                        startDragPort = connectedOutput;
                        isDrawing = true;
                    }
                }
                return;
            }

            isDrawing = true;
            startDragPort = port;
            activeTempConnection = new Connection(startDragPort, workspace);
            workspace.Insert(0, activeTempConnection);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!isDrawing || activeTempConnection == null) return;
        
            Vector2 correctLocalPos = EditorContext.Workspace.ScreenToWorkspace(evt.position);
            activeTempConnection.UpdateTempEndPosition(correctLocalPos);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!isDrawing) return;
            isDrawing = false;

            bool isConnected = false;
            Port hovered = GraphManager.HoveredPort; 

            if (hovered != null && hovered.IsInput && hovered.ParentNode != startDragPort.ParentNode)
            {
                if (hovered.DataType == startDragPort.DataType && hovered.ConnectedPorts.Count == 0)
                {
                    activeTempConnection.CompleteConnection(hovered);
                    AllConnections.Add(activeTempConnection);
                    startDragPort.ConnectTo(hovered);
                    isConnected = true;
                }
            }

            if (!isConnected) workspace.Remove(activeTempConnection);

            activeTempConnection = null;
            startDragPort = null;
        }

        public void RemoveConnectionsForNode(Node node)
        {
            List<Connection> toRemove = new List<Connection>();
            foreach (var conn in AllConnections)
            {
                if (conn.PortA.ParentNode == node || conn.PortB.ParentNode == node)
                    toRemove.Add(conn);
            }

            foreach (var conn in toRemove)
            {
                conn.PortA.Disconnect(conn.PortB);
                workspace.Remove(conn);
                AllConnections.Remove(conn);
            }
        }
    }
}