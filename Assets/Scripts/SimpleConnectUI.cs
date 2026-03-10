using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class SimpleConnectUI : MonoBehaviour
{
    UnityTransport utp;
    string ip = "127.0.0.1";

    void Start()
    {
        utp = FindObjectOfType<UnityTransport>();
        if (utp != null) ip = utp.ConnectionData.Address;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 320, 180), GUI.skin.box);
        GUILayout.Label("Server IP (client connects to this):");
        ip = GUILayout.TextField(ip);

        if (utp != null) utp.ConnectionData.Address = ip;

        GUILayout.Space(10);

        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening)
        {
            if (GUILayout.Button("Start Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Start Client")) NetworkManager.Singleton.StartClient();
        }
        else
        {
            GUILayout.Label("Running...");
            if (GUILayout.Button("Stop")) NetworkManager.Singleton.Shutdown();
        }

        GUILayout.EndArea();
    }
}