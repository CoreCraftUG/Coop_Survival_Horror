using CoreCraft.Core;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = CoreCraft.Core.Logger;

namespace CoreCraft.Networking.Steam
{
    public class SteamFriendsManager : Singleton<SteamFriendsManager>
    {
        [SerializeField] private RawImage _profilePicture;
        [SerializeField] private TMP_Text _userName;
        [SerializeField] private Transform _friendContent;
        [SerializeField] private GameObject _friendObject;

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

            foreach (Friend friend in SteamFriends.GetFriends())
            {
                if (!friend.IsOnline)
                    continue;

                if (!friend.IsAway)
                {
                    Logger.Instance.Log($"{friend.Name} is online", ELogType.Info);
                    if (friend.GameInfo.HasValue)
                        Logger.Instance.Log($"{friend.Name} {friend.GameInfo.ToString()}", ELogType.Info);
                }
                else
                    Logger.Instance.Log($"{friend.Name} is away", ELogType.Info);

                GameObject f = Instantiate(_friendObject, _friendContent);
                f.GetComponent<InviteUserNetworking>().SetSteamId(friend.Id);
                f.GetComponentInChildren<TMP_Text>().text = $"{(friend.GameInfo.HasValue ? $"<color=#2c8c04>{friend.Name}</color>" : $"{friend.Name}")}";
                AssignProfilePicture(f,friend.Id);
            }
        }

        public async void AssignProfilePicture(GameObject obj, SteamId id)
        {
            var img = await SteamFriends.GetMediumAvatarAsync(id);
            obj.GetComponentInChildren<RawImage>().texture = GetTextureFromImage(img.Value);
        }
    }
}