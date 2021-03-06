using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CoreCraft.Core;
using Steamworks;
using Unity.Networking.Transport.Error;
using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;


namespace CoreCraft.Networking
{
    public class Networking_Client_Net_Portal : Singleton<Networking_Client_Net_Portal>
    {
        [SerializeField] private string _menuScene;

        public NetworkingDisconnectReason DisconnectReason { get; private set; } = new NetworkingDisconnectReason();

        public event Action<EConnectStatus> OnConnectionFinished;

        public event Action OnNetworkTimedOut;

        private Networking_Game_Net_Portal _gameNetPortal;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _gameNetPortal = GetComponent<Networking_Game_Net_Portal>();

            _gameNetPortal.OnNetworkReadied += HandleNetworkReadied;
            _gameNetPortal.OnConnectionFinished += HandleConnectionFinished;
            _gameNetPortal.OnDisconnectReasonReceived += HandleDisconnectReasonReceived;
            NM.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }

        private void OnDestroy()
        {
            if (_gameNetPortal == null)
            {
                return;
            }

            _gameNetPortal.OnNetworkReadied -= HandleNetworkReadied;
            _gameNetPortal.OnConnectionFinished -= HandleConnectionFinished;
            _gameNetPortal.OnDisconnectReasonReceived -= HandleDisconnectReasonReceived;

            if (NM.Singleton == null)
            {
                return;
            }

            NM.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }

        public void StartClient()
        {
            Logger.Instance.Log($"Client started", ELogType.Debug);
            var payload = JsonUtility.ToJson(new NetworkingConnectionPayload()
            {
                Password = PasswordManager.Instance.GetPassword(),
                ClientGUID = Guid.NewGuid().ToString(),
                ClientScene = SceneManager.GetActiveScene().buildIndex,
                PlayerName = PlayerPrefs.GetString("PlayerName", "Missing Name"),
                SteamId = SteamClient.SteamId
            });
            Logger.Instance.Log($"Payload set: {payload}", ELogType.Debug);

            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            Logger.Instance.Log($"Payload converted to bytes:  {payloadBytes}", ELogType.Debug);

            NM.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            Logger.Instance.Log($"ConnectionData set", ELogType.Debug);

            NM.Singleton.StartClient();
            Logger.Instance.Log($"Client started", ELogType.Debug);
        }

        private void HandleNetworkReadied()
        {
            if (!NM.Singleton.IsClient)
            {
                return;
            }

            if (!NM.Singleton.IsHost)
            {
                _gameNetPortal.OnUserDisconnectRequested += HandleUserDisconnectRequested;
            }
        }

        private void HandleUserDisconnectRequested()
        {
            DisconnectReason.SetDisconnectReason(EConnectStatus.UserRequestedDisconnect);
            NM.Singleton.Shutdown();

            HandleClientDisconnect(NM.Singleton.LocalClientId);

            SceneManager.LoadScene(_menuScene);
        }

        private void HandleConnectionFinished(EConnectStatus status)
        {
            Logger.Instance.Log($"Client connection state: {status}", ELogType.Debug);
            if (status != EConnectStatus.Success)
            {
                DisconnectReason.SetDisconnectReason(status);
            }

            OnConnectionFinished?.Invoke(status);
        }

        private void HandleDisconnectReasonReceived(EConnectStatus status)
        {
            Logger.Instance.Log($"Client disconnection state: {status}", ELogType.Debug);
            DisconnectReason.SetDisconnectReason(status);
        }

        private void HandleClientDisconnect(ulong clientId)
        {
            if (!NM.Singleton.IsConnectedClient && !NM.Singleton.IsHost)
            {
                _gameNetPortal.OnUserDisconnectRequested -= HandleUserDisconnectRequested;

                if (SceneManager.GetActiveScene().name != _menuScene)
                {
                    if (!DisconnectReason.HasTransitionReason)
                    {
                        DisconnectReason.SetDisconnectReason(EConnectStatus.GenericDisconnect);
                    }

                    SceneManager.LoadScene(_menuScene);
                }
                else
                {
                    OnNetworkTimedOut?.Invoke();
                }
            }
        }
    }
}