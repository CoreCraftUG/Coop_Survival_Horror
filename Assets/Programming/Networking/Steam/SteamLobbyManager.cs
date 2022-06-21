using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft.Networking.Steam
{
    public class SteamLobbyManager : NetworkBehaviour
    {
        public static SteamLobbyManager Instance { get; private set; }

        public static Lobby? CurrentLobby;
        public static bool InLobby;

        public UnityEvent OnLobbyCreate;
        public UnityEvent OnLobbyJoin;
        public UnityEvent OnLobbyLeave;
        public UnityEvent OnLobbyInvite;


        [SerializeField] private FacepunchTransport _transport;

        private Friend _invitationFriend;
        private Lobby _invitationLobby;

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
                    Destroy(this);
                    Logger.Instance.Log($"There is more than one {this} in the scene", ELogType.Error);
                    Debug.LogError($"There is more than one {this} in the scene");
                }
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(this);

            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEnter;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoin;
            SteamMatchmaking.OnChatMessage += OnChatMassage;
            SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberDisconnected;
            SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
            SteamMatchmaking.OnLobbyInvite += OnIncomingLobbyInvite;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequest;
            NM.Singleton.OnServerStarted += HandleServerStarted;
        }

        private void OnDestroy()
        {
            try
            {
                SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
                SteamMatchmaking.OnLobbyEntered -= OnLobbyEnter;
                SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoin;
                SteamMatchmaking.OnChatMessage -= OnChatMassage;
                SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnected;
                SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberDisconnected;
                SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
                SteamMatchmaking.OnLobbyInvite -= OnIncomingLobbyInvite;
                SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequest;
                if (NM.Singleton)
                    NM.Singleton.OnServerStarted -= HandleServerStarted;
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"{e}", ELogType.Error);
                throw;
            }
        }

        private void OnIncomingLobbyInvite(Friend friend, Lobby lobby)
        {
            Logger.Instance.Log($"{friend.Name} invited you into a Game",ELogType.Debug);
            _invitationFriend = friend;
            _invitationLobby = lobby;
            OnLobbyInvite.Invoke();
        }

        public async void JoinLobbyButton()
        {
            OnGameLobbyJoinRequest(_invitationLobby, SteamClient.SteamId);
        }

        private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId id)
        {

        }

        private void OnLobbyMemberJoin(Lobby lobby, Friend friend)
        {
            Logger.Instance.Log($"{friend.Name} joint the Game",ELogType.Debug);

            if (!GameManager.Instance.GameStarted.Value)
                LobbyManager.Instance.CreateLobbyCard(friend);
        }

        private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
        {
            Logger.Instance.Log($"{friend.Name} disconnected from the Game", ELogType.Debug);
            Logger.Instance.Log($"{CurrentLobby?.Owner.Name} is now Owner of this Lobby", ELogType.Debug);
            
            if (!GameManager.Instance.GameStarted.Value)
                LobbyManager.Instance.DestroyLobbyCard(friend);
        }

        private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
        {

        }

        private void OnChatMassage(Lobby lobby, Friend friend, string message)
        {
            Logger.Instance.Log($"{friend.Name} send the message {message}", ELogType.Debug);
        }

        private async void OnGameLobbyJoinRequest(Lobby joinedLobby, SteamId id)
        {
            RoomEnter joinLobbySuccess = await joinedLobby.Join();

            if (joinLobbySuccess != RoomEnter.Success)
            {
                Logger.Instance.Log($"failed to join this lobby {joinedLobby}\n{joinLobbySuccess}",ELogType.Error);
            }
            else
            {
                Logger.Instance.Log($"Lobby successfully joined", ELogType.Debug);
                CurrentLobby = joinedLobby;
            }
        }

        private void OnLobbyCreated(Result result, Lobby lobby) //#################################################
        {
            if (result != Result.OK)
            {
                Logger.Instance.Log($"failed to create a lobby {result}", ELogType.Error);
            }
            else
            {
                Logger.Instance.Log($"Lobby successfully created", ELogType.Debug);
                OnLobbyCreate.Invoke();
            }
        }

        private void OnLobbyEnter(Lobby lobby)
        {
            Logger.Instance.Log($"Joint lobby as Client", ELogType.Debug);


            LobbyManager.Instance.CreateLobbyCard(new Friend(SteamClient.SteamId));

            foreach (Friend friend in CurrentLobby?.Members)
            {
                if (friend.Id == SteamClient.SteamId)
                    continue;

                LobbyManager.Instance.CreateLobbyCard(friend);
            }

            InLobby = true;
            OnLobbyJoin.Invoke();

            if (NM.Singleton.IsHost)
                return;

            _transport.targetSteamId = lobby.Owner.Id;
            Networking_Client_Net_Portal.Instance.StartClient();
        }

        public void CreateLobbyAsync()
        {
            bool result = Networking_Game_Net_Portal.Instance.StartHost();
            if (!result)
            {
                Logger.Instance.Log($"Server creation failed", ELogType.Error);
            }
        }

        private async void HandleServerStarted()
        {
            if (!NM.Singleton.IsHost) { return; }

            Logger.Instance.Log($"Server created starting Lobby", ELogType.Debug);
            bool result = await CreateLobby();
            if (!result)
            {
                Logger.Instance.Log($"Lobby creation failed", ELogType.Error);
            }
        }

        public static async Task<bool> CreateLobby()
        {
            try
            {
                var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(4);
                if (!createLobbyOutput.HasValue)
                {
                    Logger.Instance.Log($"Lobby created but currently not instantiated", ELogType.Error);
                    return false;
                }

                CurrentLobby = createLobbyOutput.Value;

                CurrentLobby?.SetPublic();

                CurrentLobby?.SetJoinable(true);
                
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"{e}",ELogType.Error);
                throw;
            }
        }

        public void LeaveLobby()
        {
            InLobby = false;
            try
            {
                CurrentLobby?.Leave();
                OnLobbyLeave.Invoke();
                NM.Singleton.Shutdown();
                LobbyManager.Instance.ClearLobbyCards();
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"{e}", ELogType.Error);
                throw;
            }
        }
    }
}
