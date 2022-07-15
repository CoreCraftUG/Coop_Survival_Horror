using System.Collections;
using System.Collections.Generic;
using CoreCraft.Character;
using Unity.Netcode;
using UnityEngine;
using Logger = CoreCraft.Core.Logger;

namespace CoreCraft.Networking
{
    public class PlayerSpawnManager : NetworkBehaviour
    {
        public static PlayerSpawnManager Instance { get; private set; }

        [SerializeField] private GameObject _playerObject;
        [SerializeField] private GameObject _physicsCharacter;
        [SerializeField] private GameObject _playerCamera;

        private Dictionary<ulong, GameObject> _playerCharacters = new Dictionary<ulong, GameObject>();

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

        public GameObject GetPlayerCharacterByClientId(ulong clientId)
        {
            if (_playerCharacters.ContainsKey(clientId))
            {
                return _playerCharacters[clientId];
            }

            return null;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayerCharacterServerRpc(ulong clientId, Vector3 position, Quaternion rotation)
        {
            GameObject player = Instantiate(_playerObject, position, rotation);

            NetworkObject nObj = player.GetComponent<NetworkObject>();
            if (nObj == null)
            {
                Logger.Instance.Log($"{player} is not usable as a player Character\nNetworkObject missing",
                    ELogType.Error);
                Debug.LogError($"{player} is not usable as a player Character\nNetworkObject missing");
                Destroy(player);
                return;
            }

            player.SetActive(true);
            nObj.SpawnAsPlayerObject(clientId, true);

            _playerCharacters.Add(clientId, player);

            PlayerController controller = player.GetComponent<PlayerController>();
            controller.SetSteamIdServerRpc(GameManager.Instance.GetSteamIdByClientId(clientId));

            GameObject cameraObj = Instantiate(_playerCamera, position, rotation);
            NetworkObject nCameraObj = cameraObj.GetComponent<NetworkObject>();
            if (nCameraObj == null)
            {
                Logger.Instance.Log($"{player} is not usable as a player Character\nNetworkObject missing",
                    ELogType.Error);
                Debug.LogError($"{player} is not usable as a player Character\nNetworkObject missing");
                Destroy(player);
                return;
            }
            cameraObj.SetActive(true);
            nCameraObj.SpawnWithOwnership(clientId, true);

            controller.SetCameraServerRpc(nCameraObj.NetworkObjectId);

            GameObject physicsObj = Instantiate(_physicsCharacter, position, rotation);
            NetworkObject nPhysicsObj = physicsObj.GetComponent<NetworkObject>();
            if (nPhysicsObj == null)
            {
                Logger.Instance.Log($"{player} is not usable as a player Character\nNetworkObject missing",
                    ELogType.Error);
                Debug.LogError($"{player} is not usable as a player Character\nNetworkObject missing");
                Destroy(player);
                return;
            }
            physicsObj.SetActive(true);
            nPhysicsObj.SpawnWithOwnership(clientId, true);

            controller.SetPhysicsCharacterServerRpc(nPhysicsObj.NetworkObjectId);

            EventManager.Instance.PlayerSpawned.Invoke(nObj.NetworkObjectId, clientId);
        }
    }
}