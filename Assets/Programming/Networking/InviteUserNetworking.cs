using System;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft.Networking.Steam
{
    public class InviteUserNetworking : MonoBehaviour
    {
        [SerializeField] private float _inviteTimer;
        [SerializeField] private Button _inviteButton;
        [SerializeField] private Button _showInviteButton;

        private float _tempTimer;
        private SteamId _id;
        private bool _invite;

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
                    bool result = NM.Singleton.StartHost();
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