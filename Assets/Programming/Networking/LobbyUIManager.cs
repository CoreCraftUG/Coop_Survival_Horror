using System.Collections;
using System.Collections.Generic;
using CoreCraft.Networking;
using CoreCraft.Networking.Steam;
using UnityEngine;
using UnityEngine.UI;

namespace CoreCraft
{
    public class LobbyUIManager : MonoBehaviour
    {
        [Header("Lobby Created")]
        [SerializeField] private GameObject _lobbyCanvas;
        [SerializeField] private GameObject _mainCanvas;

        [Space, Header("Lobby Join")]
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private GameObject _mainButtonPanel;

        [Space, Header("Lobby Invite")]
        [SerializeField] private GameObject _joinLobbyButton;

        [Space, Header("Buttons")]
        [SerializeField] private Button _createLobbyButton;

        void Start()
        {
            SteamLobbyManager.Instance.OnLobbyCreate.AddListener(LobbyCreated);
            SteamLobbyManager.Instance.OnLobbyInvite.AddListener(LobbyInvite);
            SteamLobbyManager.Instance.OnLobbyJoin.AddListener(LobbyJoin);
            SteamLobbyManager.Instance.OnLobbyLeave.AddListener(LobbyLeave);

            _createLobbyButton.onClick.AddListener((() =>
            {
                Networking_Game_Net_Portal.Instance.StartHost();
            }));
        }

        void Update()
        {

        }

        private void LobbyCreated()
        {
            _lobbyCanvas.SetActive(true);
            _mainCanvas.SetActive(false);
        }

        private void LobbyJoin()
        {
            _lobbyPanel.SetActive(true);
            _mainButtonPanel.SetActive(false);
        }

        private void LobbyInvite()
        {
            _lobbyCanvas.SetActive(false);
            _mainButtonPanel.SetActive(true);
            _mainCanvas.SetActive(true);
        }

        private void LobbyLeave()
        {
            _joinLobbyButton.SetActive(true);
        }
    }
}
