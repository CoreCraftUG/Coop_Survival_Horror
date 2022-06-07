using System;
using System.Collections;
using System.Collections.Generic;
using CoreCraft.Core;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft.Networking.Facepuch
{
    public enum ELobbyType
    {
        Public,
        Private,
        FriendsOnly
    }

    public class GameNetworkManager : Singleton<GameNetworkManager>
    {
        [Header("Lobby Settings")]
        [SerializeField] private int _maxPlayers;
        [SerializeField] private ELobbyType _lobbyType;

        private FacepunchTransport _transport;
        public Lobby? CurrentLobby { get; private set; } = null;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }


        private void Start()
        {
            _transport = GetComponent<FacepunchTransport>();

            SteamMatchmaking.OnLobbyCreated += SteamMatchmakingOnOnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += SteamMatchmakingOnOnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmakingOnOnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmakingOnOnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyInvite += SteamMatchmakingOnOnLobbyInvite;
            SteamMatchmaking.OnLobbyGameCreated += SteamMatchmakingOnOnLobbyGameCreated;
            SteamFriends.OnGameLobbyJoinRequested += SteamFriendsOnOnGameLobbyJoinRequested;
        }

        private void OnDestroy()
        {
            SteamMatchmaking.OnLobbyCreated -= SteamMatchmakingOnOnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= SteamMatchmakingOnOnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmakingOnOnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmakingOnOnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyInvite -= SteamMatchmakingOnOnLobbyInvite;
            SteamMatchmaking.OnLobbyGameCreated -= SteamMatchmakingOnOnLobbyGameCreated;
            SteamFriends.OnGameLobbyJoinRequested -= SteamFriendsOnOnGameLobbyJoinRequested;

            if (!NM.Singleton)
                return;

            NM.Singleton.OnClientConnectedCallback -= SingletonOnOnClientConnectedCallback;
            NM.Singleton.OnClientDisconnectCallback -= SingletonOnOnClientDisconnectCallback;
            NM.Singleton.OnServerStarted -= SingletonOnOnServerStarted;
        }

        private void OnApplicationQuit() => Disconnect();

        public async void StartHost()
        {
            NM.Singleton.OnServerStarted += SingletonOnOnServerStarted;

            if(NM.Singleton.StartHost())
                Logger.Instance.Log($"Started hosting",ELogType.Debug);
            else
                Logger.Instance.Log($"Hosting failed", ELogType.Error);

            CurrentLobby = await SteamMatchmaking.CreateLobbyAsync(_maxPlayers);
        }

        public void StartClient(SteamId id)
        {
            NM.Singleton.OnClientConnectedCallback += SingletonOnOnClientConnectedCallback;
            NM.Singleton.OnClientDisconnectCallback += SingletonOnOnClientDisconnectCallback;

            _transport.targetSteamId = id;

            if (NM.Singleton.StartClient())
                Logger.Instance.Log($"Client has joined",ELogType.Debug);
        }

        public void Disconnect()
        {
            CurrentLobby?.Leave();

            if (!NM.Singleton)
                return;

            NM.Singleton.Shutdown();
        }

        #region Netcode Callbacks
        
        private void SingletonOnOnServerStarted()
        {
            Logger.Instance.Log($"Server has started", ELogType.Debug);
        }
        private void SingletonOnOnClientConnectedCallback(ulong clientId)
        {
            Logger.Instance.Log($"Client {clientId} connected", ELogType.Debug);
        }

        private void SingletonOnOnClientDisconnectCallback(ulong clientId)
        {
            Logger.Instance.Log($"Client {clientId} disconnected", ELogType.Debug);

            NM.Singleton.OnClientConnectedCallback -= SingletonOnOnClientConnectedCallback;
            NM.Singleton.OnClientDisconnectCallback -= SingletonOnOnClientDisconnectCallback;
        }

        #endregion

        #region Steam Callbacks

        private void SteamFriendsOnOnGameLobbyJoinRequested(Lobby lobby, SteamId steamId) => StartClient(steamId);

        private void SteamMatchmakingOnOnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId)
        {

        }

        private void SteamMatchmakingOnOnLobbyInvite(Friend friend, Lobby lobby)
        {
            Logger.Instance.Log($"Invited by {friend.Name}",ELogType.Debug);
        }

        private void SteamMatchmakingOnOnLobbyMemberLeave(Lobby lobby, Friend friend)
        {

        }

        private void SteamMatchmakingOnOnLobbyMemberJoined(Lobby lobby, Friend friend)
        {

        }

        private void SteamMatchmakingOnOnLobbyEntered(Lobby lobby)
        {
            if (NM.Singleton.IsHost)
                return;

            StartClient(lobby.Id);
        }

        private void SteamMatchmakingOnOnLobbyCreated(Result result, Lobby lobby)
        {
            if (result != Result.OK)
            {
                Logger.Instance.Log($"Lobby could not be created, {result}", ELogType.Error);
                return;
            }

            switch (_lobbyType)
            {
                case ELobbyType.Public:
                    lobby.SetPublic();
                    break;
                case ELobbyType.Private:
                    lobby.SetPrivate();
                    break;
                case ELobbyType.FriendsOnly:
                    lobby.SetFriendsOnly();
                    break;
                default:
                    lobby.SetPublic();
                    break;
            }

            lobby.SetData("name", $"{SteamClient.Name}'s lobby");
            lobby.SetJoinable(true);

            Logger.Instance.Log($"Lobby has been created, {result}", ELogType.Debug);
        }

        #endregion
    }
}
