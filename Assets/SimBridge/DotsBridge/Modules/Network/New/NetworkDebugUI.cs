using UnityEngine;
using UnityEngine.UIElements;
using DotsBridge.Modules.Network;

namespace DotsBridge.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class NetworkDebugUI : MonoBehaviour
    {
        private Label _statusLabel;
        private ScrollView _playerListContainer;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.Clear(); 

            // --- 1. Настройка главного контейнера (Панели) ---
            var panel = new VisualElement();
            panel.style.width = 350;
            panel.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
            panel.style.paddingTop = 15;
            panel.style.paddingBottom = 15;
            panel.style.paddingLeft = 15;
            panel.style.paddingRight = 15;
            panel.style.borderBottomLeftRadius = 10;
            panel.style.borderBottomRightRadius = 10;
            panel.style.borderTopLeftRadius = 10;
            panel.style.borderTopRightRadius = 10;
            panel.style.position = Position.Absolute;
            panel.style.top = 20;
            panel.style.left = 20;

            var title = new Label("Multiplayer Debug Panel");
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = Color.white;
            title.style.marginBottom = 15;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(title);

            // --- 2. Поля ввода ---
            var ipField = CreateTextField("IP Address", "127.0.0.1");
            var portField = CreateTextField("Port", "7979");
            var nickField = CreateTextField("Nickname", "Player_" + Random.Range(10, 99));
            var passField = CreateTextField("Password", "");
            passField.isPasswordField = true;

            panel.Add(ipField);
            panel.Add(portField);
            panel.Add(nickField);
            panel.Add(passField);

            // --- 3. Кнопки ---
            var connectBtn = new Button(() => 
            {
                UpdateStatus("Connecting...", Color.yellow);
                new ConnectionBuilder()
                    .SetIP(ipField.value)
                    .SetPort(ushort.Parse(portField.value))
                    .SetNickname(nickField.value)
                    .SetPassword(passField.value)
                    .Connect();
            }) { text = "Connect as Client" };
            connectBtn.style.marginTop = 10;
            connectBtn.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f);
            connectBtn.style.color = Color.white;

            var hostBtn = new Button(() => 
            {
                UpdateStatus("Starting Host...", Color.cyan);
                DotsBridgeBootstrapper.Instance.CurrentRole = Role.ServerClient;
                DotsBridgeBootstrapper.Instance.InitializeNetwork();
            }) { text = "Start Host" };

            var serverBtn = new Button(() => 
            {
                UpdateStatus("Starting Dedicated Server...", Color.magenta);
                DotsBridgeBootstrapper.Instance.CurrentRole = Role.Server;
                DotsBridgeBootstrapper.Instance.InitializeNetwork();
            }) { text = "Start Dedicated Server" };

            panel.Add(connectBtn);
            panel.Add(hostBtn);
            panel.Add(serverBtn);

            // --- 4. Статус бар ---
            _statusLabel = new Label("Status: Waiting input...");
            _statusLabel.style.marginTop = 15;
            _statusLabel.style.fontSize = 14;
            _statusLabel.style.color = Color.gray;
            _statusLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            panel.Add(_statusLabel);

            // --- 5. СПИСОК ИГРОКОВ (НОВОЕ) ---
            var playersTitle = new Label("Connected Players:");
            playersTitle.style.marginTop = 15;
            playersTitle.style.marginBottom = 5;
            playersTitle.style.fontSize = 14;
            playersTitle.style.color = Color.white;
            playersTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            panel.Add(playersTitle);

            _playerListContainer = new ScrollView();
            _playerListContainer.style.height = 120; // Ограничиваем высоту, чтобы панель не росла бесконечно
            _playerListContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            _playerListContainer.style.borderTopLeftRadius = 5;
            _playerListContainer.style.borderTopRightRadius = 5;
            _playerListContainer.style.borderBottomLeftRadius = 5;
            _playerListContainer.style.borderBottomRightRadius = 5;
            _playerListContainer.style.paddingTop = 5;
            _playerListContainer.style.paddingBottom = 5;
            _playerListContainer.style.paddingLeft = 5;
            _playerListContainer.style.paddingRight = 5;
            panel.Add(_playerListContainer);

            root.Add(panel);

            // --- 6. Подписка на Ивенты ---
            ConnectionManager.OnConnectedToServer += HandleConnected;
            ConnectionManager.OnDisconnectedFromServer += HandleDisconnected;
            
            // Подписываемся на вход и выход игроков
            ConnectionManager.OnPlayerJoined += HandlePlayerJoined;
            ConnectionManager.OnPlayerLeft += HandlePlayerLeft;
            
            // Первоначальная отрисовка пустого списка
            RefreshPlayerList();
        }

        private void OnDisable()
        {
            ConnectionManager.OnConnectedToServer -= HandleConnected;
            ConnectionManager.OnDisconnectedFromServer -= HandleDisconnected;
            ConnectionManager.OnPlayerJoined -= HandlePlayerJoined;
            ConnectionManager.OnPlayerLeft -= HandlePlayerLeft;
        }

        // --- Вспомогательные методы UI ---

        private TextField CreateTextField(string label, string defaultValue)
        {
            var field = new TextField(label) { value = defaultValue };
            field.style.marginBottom = 5;
            field.labelElement.style.color = Color.white;
            field.labelElement.style.minWidth = 80;
            return field;
        }

        private void UpdateStatus(string text, Color color)
        {
            if (_statusLabel == null) return;
            _statusLabel.text = $"Status: {text}";
            _statusLabel.style.color = color;
        }

        // --- Обновление списка игроков ---
        private void RefreshPlayerList()
        {
            if (_playerListContainer == null) return;
            
            _playerListContainer.Clear();

            if (ConnectionManager.Players.Count == 0)
            {
                var emptyLabel = new Label("No players connected.");
                emptyLabel.style.color = Color.gray;
                emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                _playerListContainer.Add(emptyLabel);
                return;
            }

            foreach (var kvp in ConnectionManager.Players)
            {
                var session = kvp.Value;
                // Формат: [ID] Nickname
                var playerLabel = new Label($"[{session.NetworkId}] {session.Nickname}");
                playerLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
                playerLabel.style.marginBottom = 2;
                _playerListContainer.Add(playerLabel);
            }
        }

        // --- Обработчики ивентов ---
        private void HandleConnected() => UpdateStatus("Connected & Authenticated!", Color.green);
        
        private void HandleDisconnected(string reason)
        {
            UpdateStatus($"Disconnected: {reason}", Color.red);
            RefreshPlayerList(); // Очистит список при разрыве связи
        }

        private void HandlePlayerJoined(PlayerSession session)
        {
            UpdateStatus($"Player {session.Nickname} joined!", Color.cyan);
            RefreshPlayerList();
        }

        private void HandlePlayerLeft(int id, string reason)
        {
            UpdateStatus($"Player ID {id} left ({reason})", new Color(1f, 0.6f, 0f));
            RefreshPlayerList();
        }
    }
}