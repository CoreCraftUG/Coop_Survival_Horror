using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = CoreCraft.Core.Logger;

namespace CoreCraft.Networking
{
    public class PlayerManager : NetworkBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        [SerializeField] private int _sceneBuildId;

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
            PlayerSpawnManager.Instance.SpawnPlayerCharacterServerRpc(NetworkManager.Singleton.LocalClientId, new Vector3(0, 2, 0), Quaternion.identity);
            Networking_Game_Net_Portal.Instance.OnClientSceneChanged += OnClientSceneChanged;
        }

        private void OnClientSceneChanged(ulong clientId, int sceneBuildId)
        {
            if (sceneBuildId == _sceneBuildId)
            {
                PlayerSpawnManager.Instance.SpawnPlayerCharacterServerRpc(clientId, new Vector3(0, 2, 0), Quaternion.identity);
            }
        }

        void Update()
        {
        
        }
    }
}
