using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreCraft.Networking;
using CoreCraft.Networking.Steam;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;

using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private GameObject _inLobbyFriend;
        [SerializeField] private Transform _inLobbyContent;

        public Dictionary<SteamId, GameObject> _inLobby = new Dictionary<SteamId, GameObject>();

        public static LobbyManager Instance { get; private set; }

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

        void Start()
        {
        
        }

        void Update()
        {
        
        }

        public void CreateLobbyCard(Friend friend, ulong clientId)
        {
            if (_inLobby.ContainsKey(friend.Id))
                return;

            GameObject obj = Instantiate(_inLobbyFriend, _inLobbyContent);
            obj.GetComponent<PlayerInfo>().SetUpPanel(clientId);
            obj.GetComponentInChildren<TMP_Text>().text = friend.Name;
            SteamFriendsManager.Instance.AssignProfilePicture(obj, friend.Id);
            _inLobby.Add(friend.Id, obj);
        }

        public void DestroyLobbyCard(Friend friend)
        {
            if (_inLobby.ContainsKey(friend.Id))
            {
                Destroy(_inLobby[friend.Id]);
                _inLobby.Remove(friend.Id);
            }
        }

        public void ClearLobbyCards()
        {
            foreach (SteamId id in _inLobby.Keys)
            {
                Destroy(_inLobby[id]);
            }
            _inLobby.Clear();
        }
    }
}
