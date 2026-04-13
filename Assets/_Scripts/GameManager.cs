using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private MultiplayerUI _multiplayerUI;

    private void Start()
    {
        if (_multiplayerUI != null)
        {
            _multiplayerUI.OnStartHost += OnStartHost;
            _multiplayerUI.OnStartClient += OnStartClient;
            _multiplayerUI.OnDisconnectClient += OnDisconnectClient;
        }
    }

    private void OnStartHost()
    {
        _multiplayerUI.DisableButtons();
        NetworkManager.Singleton.StartHost();
    }

    private void OnStartClient()
    {
        _multiplayerUI.DisableButtons();
        NetworkManager.Singleton.StartClient();
    }

    private void OnDisconnectClient()
    {
        _multiplayerUI.EnableButtons();
        NetworkManager.Singleton.Shutdown();
    }
}
