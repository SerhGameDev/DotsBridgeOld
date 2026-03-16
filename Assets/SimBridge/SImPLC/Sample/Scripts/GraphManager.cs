using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimPLS
{
    public class GraphManager : MonoBehaviour
    {
        [Header("UI References")]
        public UIDocument document;
        public VisualTreeAsset nodeUXML; 
        public VisualTreeAsset inputPortUXML;  
        public VisualTreeAsset outputPortUXML; 

        // Публичная статика для доступа к списку нод (для магнита) и наведенному порту
        public static IReadOnlyList<Node> Nodes => instance?.nodeManager.AllNodes;
        public static Port HoveredPort { get; set; }
        private static GraphManager instance;

        // Наши чистые подсистемы
        private SelectionManager selectionManager;
        private ConnectionManager connectionManager;
        private NodeManager nodeManager;

        private VisualElement workspace;
        private ContextMenu contextMenu;

        private void OnEnable()
        {
            instance = this;
            if (document == null) document = GetComponent<UIDocument>();
            var root = document.rootVisualElement;
            workspace = root.Q<VisualElement>(UIConst.WORKSPACE);

            // Инициализация подсистем (Внедрение зависимостей через конструкторы)
            selectionManager = new SelectionManager();
            connectionManager = new ConnectionManager(workspace);
            nodeManager = new NodeManager(workspace, nodeUXML, inputPortUXML, outputPortUXML, selectionManager, connectionManager);

            // Настройка контекстного меню
            List<NodeCreationData> registry = new List<NodeCreationData>
            {
                new NodeCreationData("AND Gate", "Logic", typeof(NodeAND)),
                new NodeCreationData("TON Timer", "Timers", typeof(NodeTimer))
            };
            contextMenu = new ContextMenu(registry);
            contextMenu.style.display = DisplayStyle.None;
            contextMenu.OnNodeSelected += nodeManager.CreateNode;
            root.Add(contextMenu);

            // Настройка глобального инпута
            root.focusable = true;
            root.RegisterCallback<KeyDownEvent>(OnKeyDown);
            workspace.RegisterCallback<PointerDownEvent>(OnWorkspacePointerDown);
        }

        private void OnWorkspacePointerDown(PointerDownEvent evt)
        {
            if (evt.target == workspace) 
            {
                if (evt.button == 0) // Левый клик по фону
                {
                    selectionManager.Deselect();
                    contextMenu.Hide(); 
                }
                else if (evt.button == 1) // Правый клик
                {
                    Vector2 localPos = workspace.WorldToLocal(evt.position);
                    contextMenu.Show(evt.position, localPos);
                    evt.StopPropagation();
                }
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Delete && selectionManager.SelectedNode != null)
            {
                nodeManager.DeleteNode(selectionManager.SelectedNode);
            }
        }
    }
}