using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimPLS
{
    public abstract class Node : VisualElement
    {
        public Action<Node> OnNodeSelected;
        public Action<Port> OnPortDragStarted;
        public Action<Node> OnNodeMoved;
        public Vector2 LogicalPosition { get; private set; }

        public List<Port> Inputs = new List<Port>();
        public List<Port> Outputs = new List<Port>();

        private bool isDragging = false;
        private Vector2 startMousePosition;
        private Vector2 startElementPosition;

        // Настройки сетки и прилипания
        private const float GRID_SIZE = 20f;     
        private const float SNAP_THRESHOLD = 15f;   

        protected VisualTreeAsset inputPortUXML;
        protected VisualTreeAsset outputPortUXML;
        protected VisualElement inputsContainer;
        protected VisualElement outputsContainer;

        public Node(VisualTreeAsset nodeTemplate, VisualTreeAsset inP, VisualTreeAsset outP)
        {
            inputPortUXML = inP;
            outputPortUXML = outP;
            
            nodeTemplate.CloneTree(this);
            this.style.position = Position.Absolute;

            inputsContainer = this.Q<VisualElement>(UIConst.CONTAINER_IN);
            outputsContainer = this.Q<VisualElement>(UIConst.CONTAINER_OUT);

            this.RegisterCallback<PointerDownEvent>(OnPointerDown);
            this.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            this.RegisterCallback<PointerUpEvent>(OnPointerUp);
            this.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        public abstract void SetupPorts();

        public void SetPosition(float x, float y)
        {
            LogicalPosition = new Vector2(x, y);
            this.style.left = x;
            this.style.top = y;
            OnNodeMoved?.Invoke(this);
        }
      protected void AddPort(bool isInput, string portName, PortDataType dataType)
        {
            VisualTreeAsset selectedUXML = isInput ? inputPortUXML : outputPortUXML;
            if (selectedUXML == null) return;

            TemplateContainer portUI = selectedUXML.Instantiate();
            
            Label label = portUI.Q<Label>(UIConst.PORT_LABEL);
            if (label != null) label.text = portName; 

            VisualElement connector = portUI.Q<VisualElement>(UIConst.PORT_CONNECTOR) ?? portUI;

            connector.style.backgroundColor = GetColorForType(dataType);

            Port logicalPort = new Port(this, isInput, portName, dataType, connector);
            if (isInput) Inputs.Add(logicalPort);
            else Outputs.Add(logicalPort);

            connector.RegisterCallback<PointerDownEvent>(evt => 
            {
                if (evt.button == 0)
                {
                    OnPortDragStarted?.Invoke(logicalPort);
                    evt.StopPropagation();
                }
            });

            connector.RegisterCallback<MouseEnterEvent>(evt => EditorContext.HoveredPort = logicalPort);
            connector.RegisterCallback<MouseLeaveEvent>(evt => {
                if (EditorContext.HoveredPort == logicalPort) EditorContext.HoveredPort = null;
            });

            if (isInput && inputsContainer != null) inputsContainer.Add(portUI);
            else if (!isInput && outputsContainer != null) outputsContainer.Add(portUI);
        }

        private Color GetColorForType(PortDataType type)
        {
            return type switch
            {
                PortDataType.Flow => new Color(1f, 1f, 1f),       // Белый
                PortDataType.Bool => new Color(0.8f, 0.2f, 0.2f), // Красный
                PortDataType.Int => new Color(0.2f, 0.8f, 0.2f),  // Зеленый
                PortDataType.Float => new Color(0.2f, 0.4f, 0.8f),// Синий
                _ => Color.gray
            };
        }

    private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                OnNodeSelected?.Invoke(this);
                isDragging = true;
                startMousePosition = evt.position;
                
                // Запоминаем стартовую ЛОГИЧЕСКУЮ позицию
                startElementPosition = LogicalPosition; 
                
                this.CapturePointer(evt.pointerId);
                this.BringToFront();
                evt.StopPropagation();
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!isDragging || !this.HasPointerCapture(evt.pointerId)) return;

            // --- ИСПРАВЛЕНО: Делим дельту на зум, чтобы скорость была 1:1 ---
            Vector2 delta = (evt.position - (Vector3)startMousePosition) / EditorContext.Zoom;
            
            float rawX = startElementPosition.x + delta.x;
            float rawY = startElementPosition.y + delta.y;

            // 1. ПРИВЯЗКА К СЕТКЕ (Grid Snapping)
            float snappedX = Mathf.Round(rawX / GRID_SIZE) * GRID_SIZE;
            float snappedY = Mathf.Round(rawY / GRID_SIZE) * GRID_SIZE;

            foreach (var otherNode in EditorContext.Nodes)
            {
                if (otherNode == this) continue;

                if (Mathf.Abs(snappedX - otherNode.LogicalPosition.x) < SNAP_THRESHOLD)
                    snappedX = otherNode.LogicalPosition.x;

                if (Mathf.Abs(snappedY - otherNode.LogicalPosition.y) < SNAP_THRESHOLD)
                    snappedY = otherNode.LogicalPosition.y;
            }

            SetPosition(snappedX, snappedY);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (isDragging) 
            { 
                isDragging = false; 
                this.ReleasePointer(evt.pointerId); 
                evt.StopPropagation(); 
            }
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt) { isDragging = false; }

        public void SetSelectedStyle(bool isSelected)
        {
            VisualElement innerNode = this.Q<VisualElement>(UIConst.NODE_ROOT);
            if (innerNode != null)
            {
                if (isSelected) innerNode.AddToClassList("node-selected");
                else innerNode.RemoveFromClassList("node-selected");
            }
        }
        
    }

    public class NodeAND : Node
    {
        public NodeAND(VisualTreeAsset n, VisualTreeAsset i, VisualTreeAsset o) : base(n, i, o) 
        {
            var title = this.Q<Label>(UIConst.NODE_TITLE);
            if (title != null) title.text = "AND Gate";
        }
        public override void SetupPorts()
        {
            // Логическое И работает с Bool
            AddPort(true, "In A", PortDataType.Bool);
            AddPort(true, "In B", PortDataType.Bool);
            AddPort(false, "Out", PortDataType.Bool);
        }
    }

    public class NodeTimer : Node
    {
        public NodeTimer(VisualTreeAsset n, VisualTreeAsset i, VisualTreeAsset o) : base(n, i, o) 
        {
            var title = this.Q<Label>(UIConst.NODE_TITLE);
            if (title != null) title.text = "TON Timer";
        }
        public override void SetupPorts()
        {
            AddPort(true, "IN", PortDataType.Int);
            AddPort(false, "Q", PortDataType.Bool);
            AddPort(false, "ET", PortDataType.Int); // Elapsed Time (целое число миллисекунд)
        }
    }
}