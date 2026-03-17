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

        private IDEUIView _uiView;
        private Hierarchy _hierarchyModel;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;
            
            _uiView = new IDEUIView(root, _hierarchyWindowTemplate, _fileTemplate, _folderTemplate);

            _hierarchyModel = new Hierarchy(_uiView.HierarchyView);

            _hierarchyModel.OnFileOpened += OnFileOpened;
            _hierarchyModel.OnSelectionChanged += OnSelectionChanged;

            PopulateTestData();
        }

        private void PopulateTestData()
        {
            
            _hierarchyModel.CreateFile("PlayerNode");
            _hierarchyModel.CreateFile("WeaponNode");

            _hierarchyModel.CreateFile("GameManagerNode");
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
            }

            _uiView?.Dispose();
        }
    }
}