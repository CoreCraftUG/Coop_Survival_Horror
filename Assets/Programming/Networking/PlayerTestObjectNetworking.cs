using System.Collections;
using System.Collections.Generic;
using CoreCraft.Networking.Steam;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Logger = CoreCraft.Core.Logger;
using Vector3 = UnityEngine.Vector3;

namespace CoreCraft.Networking
{
    public class PlayerTestObjectNetworking : NetworkBehaviour
    {
        [SerializeField] private RawImage _ppImage;
        [SerializeField] private float _speed;
        [SerializeField] private NetworkVariable<Vector3> _networkPosition;
        private CharacterController _characterController;

        private void Start()
        {
            SteamFriendsManager.Instance.AssignProfilePicture(_ppImage.gameObject,SteamClient.SteamId);
            _characterController = GetComponent<CharacterController>();
        }


        private void Update()
        {
            if (IsClient && IsOwner)
            {
                Vector3 position = new Vector3(UnityEngine.Input.GetAxis("Vertical"), UnityEngine.Input.GetAxis("Horizontal"), 0);

                UpdateClientPositionAndRotationServerRpc(position * _speed);
            }

            if (IsServer)
            {
                if (_networkPosition.Value != Vector3.zero)
                {
                    _characterController.SimpleMove(_networkPosition.Value);
                }
            }
        }

        [ServerRpc]
        public void UpdateClientPositionAndRotationServerRpc(Vector3 position)
        {
            _networkPosition.Value = position;
        }
    }
}
