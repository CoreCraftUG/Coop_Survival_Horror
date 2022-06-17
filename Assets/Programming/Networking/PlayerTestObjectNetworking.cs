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
                Vector3 position = new Vector3(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical"), 0);

                UpdateClientPositionServerRpc(/*transform.position +*/ position * _speed * Time.deltaTime);
            }

            if (IsServer)
            {
                if (_networkPosition.Value != Vector3.zero)
                {
                    _characterController.Move(_networkPosition.Value);
                }
            }

            if (IsHost && IsOwner)
            {
                if (UnityEngine.Input.GetKey(KeyCode.Space))
                {
                    Networking_Server_Net_Portal.Instance.EndRound();
                }
            }
        }

        [ServerRpc]
        public void UpdateClientPositionServerRpc(Vector3 position)
        {
            _networkPosition.Value = position;
        }
    }
}
