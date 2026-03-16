using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimPLS
{
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorContext : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument document;
    public VisualTreeAsset nodeUXML; 
    public VisualTreeAsset inputPortUXML;  
    public VisualTreeAsset outputPortUXML; 

    [Header("Navigation Settings")]
    public float minZoom = 0.2f;
    public float maxZoom = 2.0f;
    public float zoomSpeed = 0.05f;

    public static IReadOnlyList<Node> Nodes => instance?.nodeManager.AllNodes;
    public static Port HoveredPort { get; set; }
    private static EditorContext instance;

    // Подсистемы
    private WorkspaceManager workspaceManager;
    private SelectionManager selectionManager;
    private ConnectionManager connectionManager;
    private NodeManager nodeManager;

    private VisualElement workspaceViewport; // Само окно с overflow: hidden
    private ContextMenu contextMenu;

    private void OnEnable()
    {
        instance = this;
        if (document == null) document = GetComponent<UIDocument>();
        var root = document.rootVisualElement;
        
        workspaceViewport = root.Q<VisualElement>(UIConst.WORKSPACE);
        // Убеждаемся, что ноды не будут вылезать за границы вьюпорта
        workspaceViewport.style.overflow = Overflow.Hidden; 

        // 1. Инициализация навигации
        workspaceManager = new WorkspaceManager(workspaceViewport, minZoom, maxZoom, zoomSpeed);

        // 2. Инициализация остальных систем
        selectionManager = new SelectionManager();
        
        // ПЕРЕДАЕМ CONTENT CONTAINER, а не Viewport!
        connectionManager = new ConnectionManager(workspaceManager.ContentContainer);
        nodeManager = new NodeManager(
            workspaceManager.ContentContainer, // Сюда!
            nodeUXML, inputPortUXML, outputPortUXML, 
            selectionManager, connectionManager
        );

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

        root.focusable = true;
        root.RegisterCallback<KeyDownEvent>(OnKeyDown);
        
        // Клик по фону слушаем на вьюпорте
        workspaceViewport.RegisterCallback<PointerDownEvent>(OnWorkspacePointerDown);
    }

    private void OnWorkspacePointerDown(PointerDownEvent evt)
    {
        if (evt.target == workspaceViewport || evt.target == workspaceManager.ContentContainer) 
        {
            if (evt.button == 0) 
            {
                selectionManager.Deselect();
                contextMenu.Hide(); 
            }
            else if (evt.button == 1) 
            {
                // Конвертируем экранные координаты в координаты КОНТЕНТА с учетом зума
                Vector2 localPos = workspaceManager.ContentContainer.WorldToLocal(evt.position);
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