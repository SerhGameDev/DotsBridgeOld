using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace IDE
{
    [RequireComponent(typeof(UIDocument))]
    public class IDEUIDocument : MonoBehaviour
    {
        [Header("UXML Templates")]
        [SerializeField] private VisualTreeAsset _hierarchyWindowTemplate;
        [SerializeField] private VisualTreeAsset _fileTemplate;
        [SerializeField] private VisualTreeAsset _folderTemplate;
        [SerializeField] private VisualTreeAsset _contextMenuTemplate;
        [SerializeField] private VisualTreeAsset _contextMenuItemTemplate;
        [SerializeField] private VisualTreeAsset _nodeTemplate;
        
        private WorkspaceRenderer _workspaceRenderer;
        private IDEUIView _uiView;
        private Hierarchy _hierarchyModel;
        private HierarchyContextMenu _contextMenu;
        private WorkArea _activeWorkArea;
        
        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;
            
            _uiView = new IDEUIView(root, _hierarchyWindowTemplate, _fileTemplate, _folderTemplate);
            _uiView.HierarchyView.RegisterBackgroundContextTrigger(root.Q<VisualElement>("hierarchy-container"));
            _uiView.HierarchyView.RegisterBackgroundContextTrigger(root.Q<VisualElement>("hierarchy-scroll-view"));
            root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            var workspaceContainer = root.Q<VisualElement>("workspace-container");
            
            _hierarchyModel = new Hierarchy(_uiView.HierarchyView);
            _workspaceRenderer = new WorkspaceRenderer(workspaceContainer, _nodeTemplate);
            _contextMenu = new HierarchyContextMenu(root, _contextMenuTemplate, _contextMenuItemTemplate, _hierarchyModel);
            
            _hierarchyModel.OnFileOpened += HandleFileOpened;
            _hierarchyModel.OnFileOpened += OnFileOpened;
            _hierarchyModel.OnSelectionChanged += OnSelectionChanged;
            _hierarchyModel.OnItemContextRequested += HandleContextRequested;
            
            _hierarchyModel.OnFileOpened += id => 
            {
                var area = _hierarchyModel.GetWorkArea(id);
                if (area != null) _workspaceRenderer.Render(area);
            };
        }


        private void HandleFileOpened(string id)
        {
            _activeWorkArea = _hierarchyModel.GetWorkArea(id);
            if (_activeWorkArea != null)
            {
                _workspaceRenderer.Render(_activeWorkArea);
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (_activeWorkArea == null) return;

            // actionKey - это кроссплатформенный модификатор (Ctrl на Windows, Cmd на Mac)
            if (evt.actionKey)
            {
                if (evt.keyCode == KeyCode.Z)
                {
                    _activeWorkArea.History.Undo();
                    evt.StopPropagation(); // Останавливаем событие
                }
                else if (evt.keyCode == KeyCode.Y) // Можно также добавить Shift+Z для Redo
                {
                    _activeWorkArea.History.Redo();
                    evt.StopPropagation();
                }
            }
            if (evt.keyCode == KeyCode.N && !evt.actionKey)
            {
                var newNode = new Node(
                    System.Guid.NewGuid().ToString(), 
                    "Test Node", 
                    new Vector2(UnityEngine.Random.Range(50, 200), UnityEngine.Random.Range(50, 200))
                );
    
                // ВАЖНО: Делаем действие через Историю!
                _activeWorkArea.History.Execute(new AddNodeCommand(_activeWorkArea, newNode));
            }
        }
        private void HandleContextRequested(string id, Vector2 position)
        {
            Debug.Log($"[IDE] Context Requested: {id}");
            _contextMenu.ShowContextMenu(position, id);
        }

        private void OnFileOpened(string id)
        {
            Debug.Log($"[IDE] File Opened: {id}");
        }

        private void OnSelectionChanged(string id)
        {
            Debug.Log($"[IDE] Selection Changed to: {(string.IsNullOrEmpty(id) ? "None" : id)}");
        }

        private void OnDisable()
        {
            if (_hierarchyModel != null)
            {
                _hierarchyModel.OnFileOpened -= OnFileOpened;
                _hierarchyModel.OnSelectionChanged -= OnSelectionChanged;
                _hierarchyModel.Dispose();
                _hierarchyModel.OnItemContextRequested -= HandleContextRequested;
            }

            if (_contextMenu != null)
                _contextMenu?.Dispose();
            _uiView?.Dispose();
            _hierarchyModel.OnFileOpened -= HandleFileOpened;
            var root = GetComponent<UIDocument>().rootVisualElement;
            if (root != null)
            {
                root.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            }
        }
    }
}