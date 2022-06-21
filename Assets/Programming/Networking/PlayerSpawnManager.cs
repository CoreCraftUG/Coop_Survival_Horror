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

        [ServerRpc]
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

            PlayerController controller = player.GetComponent<PlayerController>();

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
            nCameraObj.SpawnAsPlayerObject(clientId, true);

            controller.SetCamera(cameraObj.GetComponent<PlayerCamera>());

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
            nPhysicsObj.SpawnAsPlayerObject(clientId, true);

            controller.SetPhysicsCharacter(physicsObj.GetComponent<PhysicsCharacter>());
        }
    }
}