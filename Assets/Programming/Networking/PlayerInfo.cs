using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft.Networking
{
    public class PlayerInfo : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _readyButton;
        [SerializeField] private Button _leaveLobbyButton;
        [SerializeField] private Button _startGameButton;

        [Space, Header("Visual")]
        [SerializeField] private Toggle _readyToggle;

        public ulong ClientId;

        private bool _ready;

        void Start()
        {
            if (ClientId == NM.Singleton.LocalClientId)
            {
                _readyButton.gameObject.SetActive(true);
                _leaveLobbyButton.gameObject.SetActive(true);

                _readyButton.onClick.AddListener(ReadyUp);
                _leaveLobbyButton.onClick.AddListener(LeaveLobby);

                if (NM.Singleton.IsHost)
                {
                    _startGameButton.onClick.AddListener(StartGame);
                }
            }
            LobbyManager.Instance.RegisterPlayerInfo(ClientId, this);
        }

        private void StartGame()
        {
            if (NM.Singleton.IsHost)
            {
                //Start Game
            }
        }

        private void LeaveLobby()
        {
            Networking_Game_Net_Portal.Instance.RequestDisconnect();
        }

        private void ReadyUp()
        {
            _ready = !_ready;
            _readyToggle.isOn = _ready;
            LobbyManager.Instance.ReadyUpServerRpc(ClientId, _ready);
        }

        public void HostReady(bool ready)
        {
            if (NM.Singleton.IsHost)
            {
                _startGameButton.gameObject.SetActive(ready);
            }
        }

        public void Ready(bool ready)
        {
            _readyToggle.isOn = ready;
        }
    }
}
