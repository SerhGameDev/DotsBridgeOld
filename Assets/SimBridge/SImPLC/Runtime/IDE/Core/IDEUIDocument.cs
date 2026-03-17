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
        private WorkspaceRenderer _workspaceRenderer;
        private IDEUIView _uiView;
        private Hierarchy _hierarchyModel;
        private HierarchyContextMenu _contextMenu;
        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;
            
            _uiView = new IDEUIView(root, _hierarchyWindowTemplate, _fileTemplate, _folderTemplate);
            _uiView.HierarchyView.RegisterBackgroundContextTrigger(root.Q<VisualElement>("hierarchy-container"));
            _uiView.HierarchyView.RegisterBackgroundContextTrigger(root.Q<VisualElement>("hierarchy-scroll-view"));
            var workspaceContainer = root.Q<VisualElement>("workspace-container");
            
            _hierarchyModel = new Hierarchy(_uiView.HierarchyView);
            _workspaceRenderer = new WorkspaceRenderer(workspaceContainer);
            _contextMenu = new HierarchyContextMenu(root, _contextMenuTemplate, _contextMenuItemTemplate, _hierarchyModel);
            
            _hierarchyModel.OnFileOpened += OnFileOpened;
            _hierarchyModel.OnSelectionChanged += OnSelectionChanged;
            _hierarchyModel.OnItemContextRequested += HandleContextRequested;
            
            _hierarchyModel.OnFileOpened += id => 
            {
                // Нам нужно получить WorkArea по ID из иерархии
                // Для этого в классе Hierarchy добавьте публичный метод GetWorkArea(id)
                var area = _hierarchyModel.GetWorkArea(id);
                if (area != null) _workspaceRenderer.Render(area);
            };
            PopulateTestData();
        }

        private void PopulateTestData()
        {
            string mainFolderId = _hierarchyModel.CreateFolder("Main Scripts");
            
            _hierarchyModel.CreateFile("PlayerNode", mainFolderId);
            _hierarchyModel.CreateFile("WeaponNode", mainFolderId);

            _hierarchyModel.CreateFile("GameManagerNode");
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
        }
    }
}