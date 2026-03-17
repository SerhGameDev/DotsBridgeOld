using UnityEngine.UIElements;

namespace IDE
{
    public class HierarchyViewElementFile : HierarchyViewElement
    {
        // Отображает имя файла
        private const string NameLabelName = "file-name-label"; 
        // Отображает номер порядка вызова
        private const string OrderLabelName = "file-order-label"; 

        private readonly Label _nameLabel;
        private readonly Label _orderLabel;

        public HierarchyViewElementFile(VisualElement rootElement, IHierarchyItemData data) 
            : base(rootElement, data)
        {
            _nameLabel = Root.Q<Label>(NameLabelName);
            _orderLabel = Root.Q<Label>(OrderLabelName);
            
            SetName(data.Name);
            SetExecutionOrder(data.ExecutionOrder);
        }

        public override void SetName(string name)
        {
            if (_nameLabel != null) _nameLabel.text = name;
        }

        public override void SetExecutionOrder(int order)
        {
            if (_orderLabel != null) _orderLabel.text = order.ToString();
        }
    }
}