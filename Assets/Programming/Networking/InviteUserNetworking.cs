using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft.Networking.Steam
{
    public class InviteUserNetworking : MonoBehaviour
    {
        [Header("System")]
        [SerializeField] private float _inviteTimer;
        [SerializeField] private Button _inviteButton;
        [SerializeField] private Button _showInviteButton;

        [Space, Header("Visual")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Image _ppFrame;

        [SerializeField] private Color _sameGameColor;
        [SerializeField] private Color _onlineColor;
        [SerializeField] private Color _otherGameColor;
        [SerializeField] private Color _awayColor;
        [SerializeField] private Color _offlineColor;

        private float _tempTimer;
        private SteamId _id;
        private bool _invite;
        private Friend _self;

        private void Start()
        {
            _showInviteButton.onClick.AddListener(ShowInviteButton);
            _inviteButton.onClick.AddListener(Invite);
            NM.Singleton.OnServerStarted += HandleServerStarted;
        }

        private void OnDestroy()
        {
            if (NM.Singleton)
                NM.Singleton.OnServerStarted -= HandleServerStarted;
        }
        
        private void Update()
        {
            if (_tempTimer < _inviteTimer)
            {
                _tempTimer += Time.deltaTime;
            }
            else
            {
                _inviteButton.gameObject.SetActive(false);
                _showInviteButton.gameObject.SetActive(true);
                _tempTimer = 0f;
            }
        }

        public void SetupPanel(Friend self, EFriendState state)
        {
            _self = self;
            switch (state)
            {
                case EFriendState.SameGame:
                    _nameText.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(_sameGameColor)}>{self.Name}</color>";
                    _ppFrame.color = _sameGameColor;
                    break;
                case EFriendState.OtherGame:
                    _nameText.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(_otherGameColor)}>{self.Name}</color>";
                    _ppFrame.color = _otherGameColor;
                    break;
                case EFriendState.Away:
                    _nameText.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(_awayColor)}>{self.Name}</color>";
                    _ppFrame.color = _awayColor;
                    break;
                case EFriendState.Online:
                    _nameText.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(_onlineColor)}>{self.Name}</color>";
                    _ppFrame.color = _onlineColor;
                    break;
                case EFriendState.Offline:
                    _nameText.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(_offlineColor)}>{self.Name}</color>";
                    _ppFrame.color = _offlineColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
            SteamFriendsManager.Instance.AssignProfilePicture(this.gameObject, self.Id);
        }

        public void SetSteamId(SteamId id)
        {
            this._id = id;
        }

        private void ShowInviteButton()
        {
            _inviteButton.gameObject.SetActive(true);
            _showInviteButton.gameObject.SetActive(false);
        }

        private void Invite()
        {
            try
            {
                if (SteamLobbyManager.InLobby)
                {
                    Logger.Instance.Log($"Invited {_id}", ELogType.Debug);
                    SteamLobbyManager.CurrentLobby?.InviteFriend(_id);
                }
                else
                {
                    _invite = true;
                    bool result = Networking_Game_Net_Portal.Instance.StartHost();
                    if (!result)
                    {
                        Logger.Instance.Log($"Server creation failed", ELogType.Error);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"Failed to Invite {_id} reason: {e}", ELogType.Error);
                throw;
            }
        }

        private void HandleServerStarted()
        {
            if (!NM.Singleton.IsHost || !_invite)
                return;

            Logger.Instance.Log($"Server created starting Lobby", ELogType.Debug);
            Logger.Instance.Log($"Invited {_id}", ELogType.Debug);
            SteamLobbyManager.CurrentLobby?.InviteFriend(_id);
        }
    }
}