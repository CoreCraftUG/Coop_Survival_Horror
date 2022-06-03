using System;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using Logger = CoreCraft.Core.Logger;

namespace CoreCraft.Networking.Steam
{
    public class InviteUserNetworking : MonoBehaviour
    {
        [SerializeField] private float _inviteTimer;
        [SerializeField] private Button _inviteButton;
        [SerializeField] private Button _showInviteButton;

        private float _tempTimer;
        private SteamId id;

        private void Start()
        {
            _showInviteButton.onClick.AddListener(ShowInviteButton);
            _inviteButton.onClick.AddListener(Invite);
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

        public void SetSteamId(SteamId id)
        {
            this.id = id;
        }

        private void ShowInviteButton()
        {
            _inviteButton.gameObject.SetActive(true);
            _showInviteButton.gameObject.SetActive(false);
        }

        private async void Invite()
        {
            try
            {
                if (SteamLobbyManager.InLobby)
                {
                    Logger.Instance.Log($"Invited {id}", ELogType.Debug);
                    SteamLobbyManager.CurrentLobby.InviteFriend(id);
                }
                else
                {
                    bool result = await SteamLobbyManager.CreateLobby();
                    if (result)
                    {
                        Logger.Instance.Log($"Invited {id}", ELogType.Debug);
                        SteamLobbyManager.CurrentLobby.InviteFriend(id);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"Failed to Invite {id} reason: {e}", ELogType.Error);
                throw;
            }
        }
    }
}