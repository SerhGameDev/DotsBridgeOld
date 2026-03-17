using DotsBridge;
using DotsBridge.Modules.Network;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUIController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField Nickname;
    public TMP_InputField IpInput;
    public TMP_InputField PortInput;
    public Button ConnectButton;
    public Button StartServerButton;
    public Button GetMyIpButton;
    public DotsNetworkWrapper NetworkWrapper;


    private void Start()
    {
        Nickname.text += "Player" + Random.Range(0, 9) + Random.Range(0, 9) + Random.Range(0, 9) + Random.Range(0, 9);
        //EntityBridge.OnClientConnected += SetNickName;
      
        if (NetworkWrapper != null)
        {
            IpInput.text = "127.0.0.1";
            PortInput.text = "7979";
        }

        ConnectButton.onClick.AddListener(OnConnectClick);
        StartServerButton.onClick.AddListener(OnStartServerClick);
        GetMyIpButton.onClick.AddListener(OnGetMyIpClick);
    }

    private void SetNickName(SingleEntity entity, int arg2)
    {
        Debug.Log("Player" + arg2 + " Conected");
    }


    private void OnConnectClick()
    {
        NetworkWrapper.ServerIP = IpInput.text;
        if (ushort.TryParse(PortInput.text, out ushort port))
        {
            NetworkWrapper.ServerPort = port;
        }

        DotsNetworkManager.ConnectClient(IpInput.text, port);
    }

    private void OnStartServerClick()
    {
        if (ushort.TryParse(PortInput.text, out ushort port))
        {
            NetworkWrapper.ServerPort = port;
        }

        DotsNetworkManager.StartServer(port);
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