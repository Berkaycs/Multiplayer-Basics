using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private MultiplayerUI _multiplayerUI;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private List<ResourcePallet> _resourcePallets;

    private void Start()
    {
        if (_multiplayerUI != null)
        {
            _multiplayerUI.OnStartHost += OnStartHost;
            _multiplayerUI.OnStartClient += OnStartClient;
            _multiplayerUI.OnDisconnectClient += OnDisconnectClient;
        }
    }
    
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;

        foreach (ResourcePallet resourcePallet in _resourcePallets)
        {
            resourcePallet.OnPalletFilled += OnPalletFilled;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        foreach (ResourcePallet resourcePallet in _resourcePallets)
        {
            resourcePallet.OnPalletFilled -= OnPalletFilled;
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

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.ConnectedClients[clientId].PlayerObject != null) return;

        GameObject player = Instantiate(_playerPrefab);

        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsNotCompleted)
    {
        foreach (ulong clientId in clientsCompleted)
        {
            OnClientConnected(clientId);
        }
    }

    private void OnPalletFilled()
    {
        int points = 0;

        foreach (ResourcePallet resourcePallet in _resourcePallets)
        {
            points += resourcePallet.StackedResources;
        }

        if (points >= _resourcePallets.Count * 3)
        {
            NetworkManager.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }
    }
}
