using System;
using System.Collections;
using System.Collections.Generic;
using CoreCraft.Core;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft.Networking
{
    public enum EConnectStatus
    {
        Undefined,
        Success,
        ServerFull,
        GameInProgress,
        LoggedInAgain,
        UserRequestedDisconnect,
        GenericDisconnect,
        PasswordIncorrect
    }

    public class Networking_Game_Net_Portal : Singleton<Networking_Game_Net_Portal>
    {
        public event Action OnNetworkReadied;

        public event Action<EConnectStatus> OnConnectionFinished;
        public event Action<EConnectStatus> OnDisconnectReasonReceived;

        public event Action<ulong, int> OnClientSceneChanged;

        public event Action OnUserDisconnectRequested;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            NM.Singleton.OnServerStarted += HandleNetworkReady;
            NM.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }

        private void OnDestroy()
        {
            if (NM.Singleton != null)
            {
                NM.Singleton.OnServerStarted -= HandleNetworkReady;
                NM.Singleton.OnClientConnectedCallback -= HandleClientConnected;

                if (NM.Singleton.SceneManager != null)
                {
                    NM.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
                }

                if (NM.Singleton.CustomMessagingManager == null) { return; }

                UnregisterClientMessageHandlers();
            }
        }

        public void StartHost()
        {
            NM.Singleton.StartHost();

            RegisterClientMessageHandlers();
        }

        public void RequestDisconnect()
        {
            OnUserDisconnectRequested?.Invoke();
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (clientId != NM.Singleton.LocalClientId) { return; }

            HandleNetworkReady();
            NM.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
        }

        private void HandleSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;

            OnClientSceneChanged?.Invoke(sceneEvent.ClientId, SceneManager.GetSceneByName(sceneEvent.SceneName).buildIndex);
        }

        private void HandleNetworkReady()
        {
            if (NM.Singleton.IsHost)
            {
                OnConnectionFinished?.Invoke(EConnectStatus.Success);
            }

            OnNetworkReadied?.Invoke();
        }

        #region Message Handlers

        private void RegisterClientMessageHandlers()
        {
            NM.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ServerToClientConnectResult", (senderClientId, reader) =>
            {
                reader.ReadValueSafe(out EConnectStatus status);
                OnConnectionFinished?.Invoke(status);
            });

            NM.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ServerToClientSetDisconnectReason", (senderClientId, reader) =>
            {
                reader.ReadValueSafe(out EConnectStatus status);
                OnDisconnectReasonReceived?.Invoke(status);
            });
        }

        private void UnregisterClientMessageHandlers()
        {
            NM.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler("ServerToClientConnectResult");
            NM.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler("ServerToClientSetDisconnectReason");
        }

        #endregion

        #region Message Senders

        public void ServerToClientConnectResult(ulong netId, EConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(EConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);
            NM.Singleton.CustomMessagingManager.SendNamedMessage("ServerToClientConnectResult", netId, writer);
        }

        public void ServerToClientSetDisconnectReason(ulong netId, EConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(EConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);
            NM.Singleton.CustomMessagingManager.SendNamedMessage("ServerToClientSetDisconnectReason", netId, writer);
        }

        #endregion
    }
}