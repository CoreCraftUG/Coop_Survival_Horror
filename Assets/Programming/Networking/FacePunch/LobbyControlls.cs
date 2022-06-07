using System.Collections;
using System.Collections.Generic;
using CoreCraft.Networking.Facepuch;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.UI;

using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft
{
    public class LobbyControlls : MonoBehaviour
    {
        [Header("Lobby Button")]
        [SerializeField] private Button _createLobbyButton;
        [SerializeField] private Button _joinLobbyButton;
        [SerializeField] private Button _leaveLobbyButton;

        [Space]
        [SerializeField] private GameObject _lobbyInactivePanel;
        [SerializeField] private GameObject _lobbyActivePanel;

        private Lobby? _invitedLobby;
        
        void Start()
        {
            _createLobbyButton.onClick.AddListener(CreateLobby);
            _joinLobbyButton.onClick.AddListener(JoinLobby);
            _leaveLobbyButton.onClick.AddListener(LeaveLobby);
            SteamMatchmaking.OnLobbyInvite += SteamMatchmakingOnOnLobbyInvite;
            SteamMatchmaking.OnLobbyEntered += SteamMatchmakingOnOnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmakingOnOnLobbyMemberJoined;
        }

        private void OnDestroy()
        {
            SteamMatchmaking.OnLobbyInvite -= SteamMatchmakingOnOnLobbyInvite;
            SteamMatchmaking.OnLobbyEntered -= SteamMatchmakingOnOnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmakingOnOnLobbyMemberJoined;
        }

        private void SteamMatchmakingOnOnLobbyEntered(Lobby lobby)
        {
            _lobbyActivePanel.SetActive(true);
            _lobbyInactivePanel.SetActive(false);
            LobbyPlayerNames.Instance.CreateNamePanel(SteamClient.Name);
        }

        private void SteamMatchmakingOnOnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            LobbyPlayerNames.Instance.CreateNamePanel(friend.Name);
        }

        private void SteamMatchmakingOnOnLobbyInvite(Friend friend, Lobby lobby)
        {
            _invitedLobby = lobby;
            Logger.Instance.Log($"{friend.Name} invited to into a lobby",ELogType.Debug);
            _joinLobbyButton.gameObject.SetActive(true);
        }

        private void CreateLobby()
        {
            GameNetworkManager.Instance.StartHost();
        }

        private void JoinLobby()
        {
            if (_invitedLobby.HasValue)
            {
                GameNetworkManager.Instance.StartClient(_invitedLobby.Value.Id);
            }
        }

        private void LeaveLobby()
        {
            GameNetworkManager.Instance.Disconnect();
            LobbyPlayerNames.Instance.ClearContent();
            _lobbyActivePanel.SetActive(false);
            _lobbyInactivePanel.SetActive(true);
        }

        void Update()
        {

        }
    }
}
