using System.Collections.Generic;
using System.Drawing;
using CoreCraft.Core;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Color = UnityEngine.Color;
using Logger = CoreCraft.Core.Logger;

namespace CoreCraft.Networking.Steam
{
    struct FriendStruct
    {
        public Friend Friend;
        public EFriendState FriendState;
    }

    public enum EFriendState
    {
        SameGame,
        OtherGame,
        Away,
        Online,
        Offline
    }

    public class SteamFriendsManager : Singleton<SteamFriendsManager>
    {
        [SerializeField] private RawImage _profilePicture;
        [SerializeField] private TMP_Text _userName;
        [SerializeField] private Transform _friendContent;
        [SerializeField] private GameObject _friendObject;

        [SerializeField] public bool ShowOtherGameFriends;
        [SerializeField] public bool ShowAwayFriends;
        [SerializeField] public bool ShowOfflineFriends;

        private List<FriendStruct> _friendList = new List<FriendStruct>();

        private void Start()
        {
            if (!SteamClient.IsValid)
                return;

            _userName.text = SteamClient.Name;

            AssignProfilePicture(_profilePicture.gameObject, SteamClient.SteamId);

            InitFriends();
        }

        public static Texture2D GetTextureFromImage(Steamworks.Data.Image img)
        {
            Texture2D texture = new Texture2D((int)img.Width,(int)img.Height);

            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    var pix = img.GetPixel(x, y);

                    texture.SetPixel(x,(int)img.Height - y, new Color(pix.r/255.0f,pix.g / 255.0f, pix.b / 255.0f, pix.a / 255.0f));
                }
            }

            texture.Apply();
            return texture;
        }

        public void InitFriends()
        {
            Logger.Instance.Log("Load Friend List",ELogType.Debug);

            for (int i = _friendContent.childCount; i > 0; i--)
            {
                Destroy(_friendContent.GetChild(i-1).gameObject);
            }
            _friendList.Clear();

            foreach (Friend friend in SteamFriends.GetFriends())
            {
                FriendStruct friendS = new FriendStruct();
                friendS.Friend = friend;
                if (!friend.IsOnline)
                {
                    friendS.FriendState = EFriendState.Offline;
                    _friendList.Add(friendS);
                    continue;
                }

                if (friend.IsAway)
                {
                    friendS.FriendState = EFriendState.Away;
                    _friendList.Add(friendS);
                    continue;
                }

                if (friend.IsOnline)
                {
                    if (friend.IsPlayingThisGame)
                    {
                        friendS.FriendState = EFriendState.SameGame;
                        _friendList.Add(friendS);
                        continue;
                    }

                    if (friend.GameInfo.HasValue)
                    {
                        friendS.FriendState = EFriendState.OtherGame;
                        _friendList.Add(friendS);
                        continue;
                    }
                    friendS.FriendState = EFriendState.Online;
                }
                
                _friendList.Add(friendS);
            }

            foreach (FriendStruct friend in _friendList.Where(friend => friend.FriendState == EFriendState.SameGame).ToList())
            {
                    GameObject f = Instantiate(_friendObject, _friendContent);
                    InviteUserNetworking invite = f.GetComponent<InviteUserNetworking>();
                    invite.SetSteamId(friend.Friend.Id);
                    invite.SetupPanel(friend.Friend, EFriendState.SameGame);
            }

            foreach (FriendStruct friend in _friendList.Where(friend => friend.FriendState == EFriendState.Online).ToList())
            {
                GameObject f = Instantiate(_friendObject, _friendContent);
                InviteUserNetworking invite = f.GetComponent<InviteUserNetworking>();
                invite.SetSteamId(friend.Friend.Id);
                invite.SetupPanel(friend.Friend, EFriendState.Online);
            }

            if (ShowOtherGameFriends)
            {
                foreach (FriendStruct friend in _friendList.Where(friend => friend.FriendState == EFriendState.OtherGame).ToList())
                {
                    GameObject f = Instantiate(_friendObject, _friendContent);
                    InviteUserNetworking invite = f.GetComponent<InviteUserNetworking>();
                    invite.SetSteamId(friend.Friend.Id);
                    invite.SetupPanel(friend.Friend, EFriendState.OtherGame);
                }
            }

            if (ShowAwayFriends)
            {
                foreach (FriendStruct friend in _friendList.Where(friend => friend.FriendState == EFriendState.Away).ToList())
                {
                    GameObject f = Instantiate(_friendObject, _friendContent);
                    InviteUserNetworking invite = f.GetComponent<InviteUserNetworking>();
                    invite.SetSteamId(friend.Friend.Id);
                    invite.SetupPanel(friend.Friend, EFriendState.Away);
                }
            }

            if (ShowOfflineFriends)
            {
                foreach (FriendStruct friend in _friendList.Where(friend => friend.FriendState == EFriendState.Offline).ToList())
                {
                    GameObject f = Instantiate(_friendObject, _friendContent);
                    InviteUserNetworking invite = f.GetComponent<InviteUserNetworking>();
                    invite.SetSteamId(friend.Friend.Id);
                    invite.SetupPanel(friend.Friend, EFriendState.Offline);
                }
            }
        }

        public async void AssignProfilePicture(GameObject obj, SteamId id)
        {
            var img = await SteamFriends.GetMediumAvatarAsync(id);
            obj.GetComponentInChildren<RawImage>().texture = GetTextureFromImage(img.Value);
        }
    }
}