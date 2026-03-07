using UnityEngine;
using UnityEngine.UI;
using TMPro; // Используем TextMeshPro для красоты
using System.Net;
using System.Net.Sockets;
using DotsBridge.Modules.Network;

public class NetworkUIController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField IpInput;
    public TMP_InputField PortInput;
    public Button ConnectButton;
    public Button StartServerButton;
    public Button GetMyIpButton;

    private void Start()
    {
        // Подтягиваем текущие значения из менеджера
        if (DotsNetworkManager.Instance != null)
        {
            IpInput.text = DotsNetworkManager.Instance.ServerIP;
            PortInput.text = DotsNetworkManager.Instance.ServerPort.ToString();
        }

        // Подписываемся на кнопки
        ConnectButton.onClick.AddListener(OnConnectClick);
        StartServerButton.onClick.AddListener(OnStartServerClick);
        GetMyIpButton.onClick.AddListener(OnGetMyIpClick);
    }

    private void OnConnectClick()
    {
        // Обновляем данные в менеджере перед подключением
        DotsNetworkManager.Instance.ServerIP = IpInput.text;
        if (ushort.TryParse(PortInput.text, out ushort port))
        {
            DotsNetworkManager.Instance.ServerPort = port;
        }

        DotsNetworkManager.Instance.ConnectToServer();
    }

    private void OnStartServerClick()
    {
        if (ushort.TryParse(PortInput.text, out ushort port))
        {
            DotsNetworkManager.Instance.ServerPort = port;
        }

        DotsNetworkManager.Instance.StartServer();
    }

    private void OnGetMyIpClick()
    {
        string localIp = GetLocalIPAddress();
        IpInput.text = localIp;
        Debug.Log($"[UI] Локальный IP определен: {localIp}");
    }

    // Хитрый метод для получения IPv4 именно этого компьютера
    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1"; // Если ничего не нашли
    }
}