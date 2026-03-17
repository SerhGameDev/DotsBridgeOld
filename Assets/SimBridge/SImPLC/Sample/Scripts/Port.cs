using System.Collections.Generic;
using UnityEngine.UIElements;

namespace SimPLS
{
    // --- КОНСТАНТЫ ИМЕН UI TOOLKIT ---
    public static class UIConst
    {
        // Главный экран
        public const string WORKSPACE = "workspace-area";     // Зона, где таскаются ноды
        public const string BTN_AND = "btn-add-and";          // Кнопка создания AND
        public const string BTN_TIMER = "btn-add-timer";      // Кнопка создания Timer
        
        // Внутренности Ноды (Node.uxml)
        public const string NODE_ROOT = "Node";               // Главный элемент ноды для выделения
        public const string NODE_TITLE = "node-title";        // Текст заголовка ноды
        public const string CONTAINER_IN = "inputs-container";  // Контейнер для левых портов
        public const string CONTAINER_OUT = "outputs-container";// Контейнер для правых портов

        // Внутренности Порта (InputPort.uxml / OutputPort.uxml)
        public const string PORT_LABEL = "port-label";        // Текст подписи порта
        public const string PORT_CONNECTOR = "port-connector";// Кружочек (или кнопка), откуда тянется провод
    }
    public enum PortDataType
    {
        Flow,   // Поток выполнения (обычно белые порты)
        Bool,   // Логический (красные)
        Int,    // Целочисленный (зеленые)
        Float   // Число с плавающей точкой (синие)
    }
    public class Port
    {
        public Node ParentNode { get; private set; } 
        public bool IsInput { get; private set; }         
        public string PortName { get; private set; }      
        public PortDataType DataType { get; private set; } 
        public VisualElement VisualConnector { get; private set; } 
        public List<Port> ConnectedPorts { get; private set; } = new List<Port>();

        public Port(Node parent, bool isInput, string name, PortDataType type, VisualElement visual)
        {
            ParentNode = parent;
            IsInput = isInput;
            PortName = name;
            DataType = type;
            VisualConnector = visual;
        }

        public void ConnectTo(Port otherPort)
        {
            if (!ConnectedPorts.Contains(otherPort))
            {
                ConnectedPorts.Add(otherPort);
                otherPort.ConnectedPorts.Add(this);
            }
        }

        public void Disconnect(Port otherPort)
        {
            if (ConnectedPorts.Contains(otherPort))
            {
                ConnectedPorts.Remove(otherPort);
                otherPort.ConnectedPorts.Remove(this);
            }
        }
    }
}