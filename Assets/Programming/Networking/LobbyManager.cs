using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft.Networking
{
    public class LobbyManager : NetworkBehaviour
    {
        [SerializeField] private Button _startGameButton;


        [Space, Header("Debug")]
        [SerializeField] private Button _debugClientDataButton;

        private List<NetworkingLobbyPlayerState> _lobbyPlayersServer = new List<NetworkingLobbyPlayerState>();
        private List<NetworkingLobbyPlayerState> _lobbyPlayersClient = new List<NetworkingLobbyPlayerState>();

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

        private void Start()
        {
            if (IsServer)
            {
                _startGameButton.gameObject.SetActive(true);
                _startGameButton.onClick.AddListener(OnStartGame);

                NM.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NM.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

                foreach (NetworkClient client in NM.Singleton.ConnectedClientsList)
                {
                    HandleClientConnected(client.ClientId);
                }
            }

            _debugClientDataButton.onClick.AddListener(DebugClientData);
        }

        private void OnStartGame()
        {
            throw new System.NotImplementedException();
        }

        private void HandleClientDisconnected(ulong clientId)
        {
        }

        private void HandleClientConnected(ulong clientId)
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
        }

        private void DebugClientData()
        {
            if (NM.Singleton.IsServer)
            {
                foreach (NetworkingLobbyPlayerState playerState in _lobbyPlayersServer)
                {
                    Logger.Instance.Log($"{playerState.PlayerName} Steam ID: {playerState.SteamId} Client ID: {playerState.ClientId}", ELogType.Debug);
                }
            }
        }
    }
}
