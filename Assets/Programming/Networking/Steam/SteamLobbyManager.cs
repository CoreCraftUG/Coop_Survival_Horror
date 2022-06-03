using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Logger = CoreCraft.Core.Logger;

namespace CoreCraft.Networking.Steam
{
    public class SteamLobbyManager : MonoBehaviour
    {
        public static Lobby CurrentLobby;

        public UnityEvent OnLobbyCreate;
        public UnityEvent OnLobbyJoin;
        public UnityEvent OnLobbyLeave;

        [SerializeField] private GameObject _inLobbyFriend;
        [SerializeField] private Transform _inLobbyContent;

        public Dictionary<SteamId, GameObject> _inLobby = new Dictionary<SteamId, GameObject>();

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
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequest;
            SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        }

        private void OnLobbyInvite(Friend friend, Lobby lobby)
        {
            Logger.Instance.Log($"{friend.Name} invited you into a Game",ELogType.Debug);
        }

        private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId id)
        {

        }

        private void OnLobbyMemberJoin(Lobby lobby, Friend friend)
        {
            Logger.Instance.Log($"{friend.Name} joint the Game",ELogType.Debug);

            GameObject obj = Instantiate(_inLobbyFriend, _inLobbyContent);
            obj.GetComponentInChildren<TMP_Text>().text = friend.Name;
            SteamFriendsManager.Instance.AssignProfilePicture(obj, friend.Id);
            _inLobby.Add(friend.Id, obj);
        }

        private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
        {
            Logger.Instance.Log($"{friend.Name} disconnected from the Game", ELogType.Debug);
            Logger.Instance.Log($"{CurrentLobby.Owner.Name} is now Owner of this Lobby", ELogType.Debug);

            if (_inLobby.ContainsKey(friend.Id))
            {
                Destroy(_inLobby[friend.Id]);
                _inLobby.Remove(friend.Id);
            }
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

        private void OnLobbyCreated(Result result, Lobby lobby)
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

            GameObject obj1 = Instantiate(_inLobbyFriend, _inLobbyContent);
            obj1.GetComponentInChildren<TMP_Text>().text = SteamClient.Name;
            SteamFriendsManager.Instance.AssignProfilePicture(obj1, SteamClient.SteamId);

            _inLobby.Add(SteamClient.SteamId, obj1);

            foreach (Friend friend in CurrentLobby.Members)
            {
                if (friend.Id == SteamClient.SteamId)
                    continue;

                GameObject obj2 = Instantiate(_inLobbyFriend, _inLobbyContent);
                obj2.GetComponentInChildren<TMP_Text>().text = friend.Name;
                SteamFriendsManager.Instance.AssignProfilePicture(obj2, friend.Id);

                _inLobby.Add(friend.Id, obj2);
            }

            OnLobbyJoin.Invoke();
        }

        public async void CreateLobbyAsync()
        {
            bool result = await CreateLobby();
            if (!result)
            {
                Logger.Instance.Log($"Lobby creation failed", ELogType.Error);
            }
        }

        private async Task<bool> CreateLobby()
        {
            try
            {
                var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync();
                if (!createLobbyOutput.HasValue)
                {
                    Logger.Instance.Log($"Lobby created but currently not instantiated", ELogType.Error);
                    return false;
                }

                CurrentLobby = createLobbyOutput.Value;

                CurrentLobby.SetPublic();

                CurrentLobby.SetJoinable(true);

                CurrentLobby.MaxMembers = 4;
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
            try
            {
                CurrentLobby.Leave();
                OnLobbyLeave.Invoke();
                foreach (SteamId id in _inLobby.Keys)
                {
                    Destroy(_inLobby[id]);
                }
                _inLobby.Clear();
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"{e}", ELogType.Error);
                throw;
            }
        }
    }
}
