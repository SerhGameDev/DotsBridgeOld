namespace UIBridge
{

    // === 5. Сборка (ViewModel конкретной панели) ===

    public class SmallPanelViewModel : ViewModel
    {
        private readonly ViewPosition _viewPosition;
        private FixedPanel _fixedPanel;
        private ViewText _viewText;

        public SmallPanelViewModel(ViewPosition viewPosition)
        {
            _viewPosition = viewPosition;
        }

        public override void Build()
        {
            _fixedPanel = UI.Create<FixedPanel>(FixedPanel.PrefabId);
            _viewText = UI.Create<ViewText>(ViewText.PrefabId);

            _fixedPanel.Add(_viewText);
            _viewPosition.ApplyTo(_fixedPanel);

            _fixedPanel.Show();
        }

        public override void Dispose()
        {
            // При закрытии возвращаем элементы обратно через статику
            UI.Release(_viewText);
            UI.Release(_fixedPanel);
        }
    }
}