using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreCraft.Character;
using CoreCraft.Networking.Steam;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft.Networking
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        private List<NetworkingLobbyPlayerState> _lobbyPlayersServer = new List<NetworkingLobbyPlayerState>();
        private List<NetworkingLobbyPlayerState> _lobbyPlayersClient = new List<NetworkingLobbyPlayerState>();

        private Dictionary<ulong, PlayerInfo> _playerInfo = new Dictionary<ulong, PlayerInfo>();

        public NetworkVariable<bool> GameStarted = new NetworkVariable<bool>();

        #region SyncPlayerStateList
        [ServerRpc]
        private void SyncLobbyListServerRpc()
        {
            SyncLobbyListClientRpc(_lobbyPlayersServer.ToArray());
        }

        [ClientRpc]
        private void SyncLobbyListClientRpc(NetworkingLobbyPlayerState[] playersArray)
        {
            _lobbyPlayersClient = playersArray.ToList();
        }
        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Instance != this)
                {
                    Logger.Instance.Log($"There is more than one {this} in the scene", ELogType.Error);
                    Debug.LogError($"There is more than one {this} in the scene");
                }
            }
        }

        public ulong GetClientIdBySteamFriend(Friend friend)
        {
            return _lobbyPlayersClient.Single(state => state.SteamId == friend.Id).ClientId;
        }

        private void Start()
        {
            DontDestroyOnLoad(this);

            NM.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NM.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;

            EventManager.Instance.PlayerSpawned.AddListener(OnPlayerSpawnedServerRpc);
        }

        [ServerRpc]
        private void OnPlayerSpawnedServerRpc(ulong playerId, ulong clientId)
        {
            if (_lobbyPlayersServer.Any(p => p.ClientId == clientId))
            {
                _lobbyPlayersServer.Single(p => p.ClientId == clientId).SetPlayerController(playerId);
                SyncLobbyListServerRpc();
            }
            else
            {
                Debug.LogError($"Client {clientId} not in lobby");
            }
        }

        public List<NetworkingLobbyPlayerState> GetPlayerInfos()
        {
            if (IsServer)
            {
                List<NetworkingLobbyPlayerState> returnList = _lobbyPlayersServer;
                return returnList;
            }
            else
            {
                List<NetworkingLobbyPlayerState> returnList = _lobbyPlayersClient;
                return returnList;
            }
        }

        [ServerRpc]
        public void GameStartedServerRpc(bool started)
        {
            GameStarted.Value = started;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ReadyUpServerRpc(ulong clientId, bool ready)
        {
            NetworkingLobbyPlayerState playerState = _lobbyPlayersServer.Single(state => state.ClientId == clientId);
            _lobbyPlayersServer.Remove(_lobbyPlayersServer.Single(state => state.ClientId == clientId));
            playerState.IsReady = ready;
            _lobbyPlayersServer.Add(playerState);
            SyncLobbyListServerRpc();
            ReadyUpClientRpc();
        }

        [ClientRpc]
        private void ReadyUpClientRpc()
        {
            bool ready = _playerInfo.All(pair => _lobbyPlayersClient.Single(state => state.ClientId == pair.Value.ClientId).IsReady != false);

            foreach (KeyValuePair<ulong, PlayerInfo> keyValuePair in _playerInfo)
            {
                keyValuePair.Value.Ready(_lobbyPlayersClient.Single(state => state.ClientId == keyValuePair.Key).IsReady);
                keyValuePair.Value.HostReady(ready);
            }
        }

        public void RegisterPlayerInfo(ulong clientId, PlayerInfo info)
        {
            _playerInfo.Add(clientId,info);
        }

        private void OnDestroy()
        {
            if (NM.Singleton)
            {
                NM.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NM.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
                SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            }
        }

        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            if (IsServer && result == Result.OK)
            {
                Debug.Log($"{NM.Singleton.LocalClientId}");
                PlayerData? playerData = Networking_Server_Net_Portal.Instance.GetPlayerData(0);

                if (!playerData.HasValue)
                    return;

                _lobbyPlayersServer.Add(
                    new NetworkingLobbyPlayerState(
                        playerData.Value.ClientId,
                        playerData.Value.PlayerName,
                        false,
                        playerData.Value.SteamId
                    ));

                SyncLobbyListServerRpc();

                NetworkingLobbyPlayerState state = _lobbyPlayersServer.Single(s => s.ClientId == playerData.Value.ClientId);
                LobbyManager.Instance.CreateLobbyCard(new Friend(state.SteamId), state.ClientId);
            }
        }

        public void OnStartGame()
        {
            if (IsHost)
            {
                Networking_Server_Net_Portal.Instance.StartGame();
            }
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            SteamLobbyManager.Instance.LeaveLobby();
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (IsServer)
            {
                PlayerData? playerData = Networking_Server_Net_Portal.Instance.GetPlayerData(clientId);

                if (!playerData.HasValue)
                    return;

                _lobbyPlayersServer.Add(
                    new NetworkingLobbyPlayerState(
                        playerData.Value.ClientId,
                        playerData.Value.PlayerName,
                        false,
                        playerData.Value.SteamId
                    ));

                SyncLobbyListServerRpc();



                if (!GameStarted.Value)
                {
                    NetworkingLobbyPlayerState state = _lobbyPlayersServer.Single(s => s.ClientId == clientId);
                    LobbyManager.Instance.CreateLobbyCard(new Friend(state.SteamId),state.ClientId);
                }

                CreateLobbyCardsClientRpc();
            }
        }

        [ClientRpc]
        private void CreateLobbyCardsClientRpc()
        {
            if (!IsHost)
            {
                foreach (NetworkingLobbyPlayerState state in _lobbyPlayersClient)
                {
                    LobbyManager.Instance.CreateLobbyCard(new Friend(state.SteamId), state.ClientId);
                }
            }
        }

        public void DebugClientData()
        {
            Logger.Instance.Log($"Debug client data", ELogType.Debug);
            if (NM.Singleton.IsHost)
            {
                Logger.Instance.Log($"Registered clients {_lobbyPlayersClient.Count}", ELogType.Debug);
                foreach (NetworkingLobbyPlayerState playerState in _lobbyPlayersClient)
                {
                    Logger.Instance.Log($"{playerState.PlayerName} Steam ID: {playerState.SteamId} Client ID: {playerState.ClientId} Ready: {playerState.IsReady}", ELogType.Debug);
                }
            }
        }
    }
}
