using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CoreCraft.Core;
using CoreCraft.Networking;
using CoreCraft.Networking.Steam;
using Steamworks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;

public class Networking_Server_Net_Portal : Singleton<Networking_Server_Net_Portal>
{
    private int maxPlayers;

    [Space, Header("Scenes")]
    [SerializeField] private string _menuScene;
    [SerializeField] private string _lobbyScene;
    [SerializeField] private string _gameScene;
    
    private Dictionary<string, PlayerData> clientData;
    private Dictionary<ulong, string> clientIdToGuid;
    private Dictionary<ulong, int> clientSceneMap;
    private bool gameInProgress;

    private const int MaxConnectionPayload = 1024;

    private Networking_Game_Net_Portal _gameNetPortal;

    private NetworkVariable<FixedString32Bytes> _serverPassword = new NetworkVariable<FixedString32Bytes>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        maxPlayers = SteamLobbyManager.CurrentLobby.MaxMembers;
        _gameNetPortal = GetComponent<Networking_Game_Net_Portal>();
        _gameNetPortal.OnNetworkReadied += HandleNetworkReadied;

        NM.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NM.Singleton.OnServerStarted += HandleServerStarted;

        clientData = new Dictionary<string, PlayerData>();
        clientIdToGuid = new Dictionary<ulong, string>();
        clientSceneMap = new Dictionary<ulong, int>();
    }

    private void OnDestroy()
    {
        if (_gameNetPortal == null) { return; }

        _gameNetPortal.OnNetworkReadied -= HandleNetworkReadied;

        if (NM.Singleton == null) { return; }

        NM.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        NM.Singleton.OnServerStarted -= HandleServerStarted;
    }

    public PlayerData? GetPlayerData(ulong clientId)
    {
        if (clientIdToGuid.TryGetValue(clientId, out string clientGuid))
        {
            if (clientData.TryGetValue(clientGuid, out PlayerData playerData))
            {
                return playerData;
            }
            else
            {
                Debug.LogWarning($"No player data found for client id: {clientId}");
            }
        }
        else
        {
            Debug.LogWarning($"No client guid found for client id: {clientId}");
        }

        return null;
    }

    public void StartGame()
    {
        gameInProgress = true;

        NM.Singleton.SceneManager.LoadScene(_gameScene, LoadSceneMode.Single);
    }

    public void EndRound()
    {
        gameInProgress = false;

        NM.Singleton.SceneManager.LoadScene(_lobbyScene, LoadSceneMode.Single);
    }

    private void HandleNetworkReadied()
    {
        if (!NM.Singleton.IsServer) { return; }

        _gameNetPortal.OnUserDisconnectRequested += HandleUserDisconnectRequested;
        NM.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        _gameNetPortal.OnClientSceneChanged += HandleClientSceneChanged;

        // NM.Singleton.SceneManager.LoadScene(_lobbyScene, LoadSceneMode.Single);

        if (NM.Singleton.IsHost)
        {
            clientSceneMap[NM.Singleton.LocalClientId] = SceneManager.GetActiveScene().buildIndex;
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        Logger.Instance.Log($"Client {clientId} disconnected",ELogType.Debug);
        clientSceneMap.Remove(clientId);

        if (clientIdToGuid.TryGetValue(clientId, out string guid))
        {
            clientIdToGuid.Remove(clientId);

            if (clientData[guid].ClientId == clientId)
            {
                clientData.Remove(guid);
            }
        }

        if (clientId == NM.Singleton.LocalClientId)
        {
            _gameNetPortal.OnUserDisconnectRequested -= HandleUserDisconnectRequested;
            NM.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
            _gameNetPortal.OnClientSceneChanged -= HandleClientSceneChanged;
        }
    }

    private void HandleClientSceneChanged(ulong clientId, int sceneIndex)
    {
        clientSceneMap[clientId] = sceneIndex;
    }

    private void HandleUserDisconnectRequested()
    {
        Logger.Instance.Log($"Client {NM.Singleton.LocalClientId} requests disconnection", ELogType.Debug);
        HandleClientDisconnect(NM.Singleton.LocalClientId);

        NM.Singleton.Shutdown();

        ClearData();

        SceneManager.LoadScene(_menuScene);
    }

    private void HandleServerStarted()
    {
        if (!NM.Singleton.IsHost) { return; }
        
        string clientGuid = Guid.NewGuid().ToString();
        string playerName = PlayerPrefs.GetString("PlayerName", "Missing Name");
        clientData.Add(clientGuid, new PlayerData(playerName, NM.Singleton.LocalClientId, SteamClient.SteamId));
        clientIdToGuid.Add(NM.Singleton.LocalClientId, clientGuid);
    }

    [ServerRpc]
    private void SetPasswordServerRpc()
    {
        _serverPassword.Value = PasswordManager.Instance.GetPassword();
    }

    private void ClearData()
    {
        clientData.Clear();
        clientIdToGuid.Clear();
        clientSceneMap.Clear();

        gameInProgress = false;
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, NM.ConnectionApprovedDelegate callback)
    {
        Logger.Instance.Log($"Client {clientId} requests Approval", ELogType.Debug);
        if (connectionData.Length > MaxConnectionPayload)
        {
            callback(false, 0, false, null, null);
            return;
        }

        if (clientId == NM.Singleton.LocalClientId)
        {
            callback(false, null, true, null, null);
            return;
        }

        string payload = Encoding.UTF8.GetString(connectionData);
        var connectionPayload = JsonUtility.FromJson<NetworkingConnectionPayload>(payload);

        EConnectStatus gameReturnStatus = EConnectStatus.Success;

        // This stops us from running multiple standalone builds since 
        // they disconnect eachother when trying to join
        //
        // if (clientData.ContainsKey(connectionPayload.ClientGUID))
        // {
        //     ulong oldClientId = clientData[connectionPayload.ClientGUID].ClientId;
        //     StartCoroutine(WaitToDisconnectClient(oldClientId, EConnectStatus.LoggedInAgain));
        // }

        //TODO: Return Connection Fail Reason
        if (connectionPayload.Password == _serverPassword.Value)
        {
            gameReturnStatus = EConnectStatus.PasswordIncorrect;
        }
        else if (gameInProgress)
        {
            //TODO: Reconnect Client
            gameReturnStatus = EConnectStatus.GameInProgress;
        }
        else if (clientData.Count >= maxPlayers)
        {
            gameReturnStatus = EConnectStatus.ServerFull;
        }

        if (gameReturnStatus == EConnectStatus.Success)
        {
            clientSceneMap[clientId] = connectionPayload.ClientScene;
            clientIdToGuid[clientId] = connectionPayload.ClientGUID;
            clientData[connectionPayload.ClientGUID] = new PlayerData(connectionPayload.PlayerName, clientId, connectionPayload.SteamId);
        }

        callback(false, 0, true, null, null);

        _gameNetPortal.ServerToClientConnectResult(clientId, gameReturnStatus);

        if (gameReturnStatus != EConnectStatus.Success)
        {
            StartCoroutine(WaitToDisconnectClient(clientId, gameReturnStatus));
        }
    }

    private IEnumerator WaitToDisconnectClient(ulong clientId, EConnectStatus reason)
    {
        _gameNetPortal.ServerToClientSetDisconnectReason(clientId, reason);

        yield return new WaitForSeconds(0);

        KickClient(clientId);
    }

    private void KickClient(ulong clientId)
    {
        NetworkObject networkObject = NM.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
        if (networkObject != null)
        {
            networkObject.Despawn(true);
        }

        NM.Singleton.DisconnectClient(clientId);
    }
}
