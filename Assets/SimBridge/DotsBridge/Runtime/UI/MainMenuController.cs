using UnityEngine;
using UnityEngine.UIElements;
using DotsBridge.Core;
using DotsBridge;

public class MainMenuController : MonoBehaviour
{
    private UIDocument _uiDocument;
    private VisualElement _root;
    private VisualElement _menuContainer;

    // Поля ввода
    private TextField _nicknameField;
    private TextField _passwordField;
    private TextField _ipField;
    private TextField _portField;
    
    // Кнопки
    private Button _btnHost;
    private Button _btnConnect;
    private Button _btnDisconnect;

    // Радар (Список серверов)
    private ScrollView _serverListContainer;

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        _root = _uiDocument.rootVisualElement;

        _menuContainer = _root.Q<VisualElement>("MenuContainer");

        _nicknameField = _root.Q<TextField>("InputNickname");
        _passwordField = _root.Q<TextField>("InputPassword");
        _ipField = _root.Q<TextField>("InputIP");
        _portField = _root.Q<TextField>("InputPort");

        _btnHost = _root.Q<Button>("BtnHost");
        _btnConnect = _root.Q<Button>("BtnConnect");
        _btnDisconnect = _root.Q<Button>("BtnDisconnect");
        
        // Находим контейнер для списка серверов
        _serverListContainer = _root.Q<ScrollView>("ServerListContainer");

        if (_btnHost != null) _btnHost.clicked += OnHostClicked;
        if (_btnConnect != null) _btnConnect.clicked += OnConnectClicked;
        if (_btnDisconnect != null) _btnDisconnect.clicked += OnDisconnectClicked;

        ClientBridge.OnStarted += HideMenu;
        ServerBridge.OnStarted += HideMenu;
        ClientBridge.OnStopped += ShowMenu;
        ServerBridge.OnStopped += ShowMenu;

        _nicknameField.value = PlayerPrefs.GetString("PlayerNickname", "Operator_" + Random.Range(1000, 9999));
        if (_btnDisconnect != null) _btnDisconnect.style.display = DisplayStyle.None;

        // ПОДПИСКА НА РАДАР
        if (LocalServerDiscovery.Instance != null)
        {
            LocalServerDiscovery.Instance.OnServersUpdated += RefreshServerListUI;
            RefreshServerListUI(); // Отрисовываем сразу, если кто-то уже найден
        }
    }

    private void OnDisable()
    {
        if (_btnHost != null) _btnHost.clicked -= OnHostClicked;
        if (_btnConnect != null) _btnConnect.clicked -= OnConnectClicked;
        if (_btnDisconnect != null) _btnDisconnect.clicked -= OnDisconnectClicked;

        ClientBridge.OnStarted -= HideMenu;
        ServerBridge.OnStarted -= HideMenu;
        ClientBridge.OnStopped -= ShowMenu;
        ServerBridge.OnStopped -= ShowMenu;

        // ОТПИСКА ОТ РАДАРА
        if (LocalServerDiscovery.Instance != null)
        {
            LocalServerDiscovery.Instance.OnServersUpdated -= RefreshServerListUI;
        }
    }

    // --- ЛОГИКА ОТРИСОВКИ РАДАРА ---
    private void RefreshServerListUI()
    {
        if (_serverListContainer == null) return;

        // Очищаем старый список
        _serverListContainer.Clear();

        var servers = LocalServerDiscovery.Instance.ActiveServers;

        if (servers.Count == 0)
        {
            _serverListContainer.Add(new Label("Поиск серверов в локальной сети..."));
            return;
        }

        // Создаем кнопку для каждого найденного сервера
        foreach (var kvp in servers)
        {
            var serverData = kvp.Value;

            Button serverBtn = new Button();
            // Выводим текст на кнопке: "Имя Сервера [IP:Порт]"
            serverBtn.text = $"📡 {serverData.Name} [{serverData.IP}:{serverData.Port}]";
            
            // Если захочешь стилизовать кнопки в USS, можно добавить класс:
            // serverBtn.AddToClassList("server-item-button");

            // Что происходит при клике на сервер:
            serverBtn.clicked += () => 
            {
                Debug.Log($"[UI] Выбран сервер: {serverData.IP}");
                
                // 1. Автоматически заполняем поля IP и Порта
                if (_ipField != null) _ipField.value = serverData.IP;
                if (_portField != null) _portField.value = serverData.Port.ToString();
                
                // 2. Сразу запускаем подключение!
                OnConnectClicked();
            };

            _serverListContainer.Add(serverBtn);
        }
    }

    // --- СТАРЫЕ МЕТОДЫ ОСТАЛИСЬ БЕЗ ИЗМЕНЕНИЙ ---
    private void OnHostClicked()
    {
        SavePrefs();
        var bridge = MonoBehaviourBridge.Instance;
        bridge.Network.CurrentRole = Role.ServerClient;
        
        if (!ushort.TryParse(_portField.value, out ushort port)) port = 7979;
        
        bridge.Network.ServerPort = port;
        bridge.Security.DefaultNickname = _nicknameField.value;
        bridge.Security.ServerPassword = _passwordField.value;

        bridge.InitializeNetwork();
    }

    private void OnConnectClicked()
    {
        SavePrefs();
        var bridge = MonoBehaviourBridge.Instance;
        if (!ushort.TryParse(_portField.value, out ushort port)) port = 7979;
        string ip = string.IsNullOrEmpty(_ipField.value) ? "127.0.0.1" : _ipField.value;

        bridge.StartClientFromUI(ip, port, _nicknameField.value, _passwordField.value);
    }

    private void OnDisconnectClicked() => MonoBehaviourBridge.Instance.ClearWorlds();
    private void SavePrefs() { PlayerPrefs.SetString("PlayerNickname", _nicknameField.value); PlayerPrefs.Save(); }
    
    private void HideMenu()
    {
        if (_menuContainer != null) _menuContainer.style.display = DisplayStyle.None;
        if (_btnDisconnect != null) _btnDisconnect.style.display = DisplayStyle.Flex;
    }
    private void ShowMenu()
    {
        if (_menuContainer != null) _menuContainer.style.display = DisplayStyle.Flex;
        if (_btnDisconnect != null) _btnDisconnect.style.display = DisplayStyle.None;
    }
}