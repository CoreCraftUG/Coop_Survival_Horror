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

        [SerializeField] private string _sceneName;

        private Vector3 _spawnPosition = Vector3.zero;

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
                    Destroy(this.gameObject);
                }
            }
        }

        void Start()
        {
            DontDestroyOnLoad(this);

            Networking_Game_Net_Portal.Instance.OnNetworkReadied += OnNetworkReadied;
        }

        private void OnNetworkReadied()
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += SceneLoadComplete;
        }

        private void SceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (IsServer && sceneName == _sceneName && PlayerSpawnManager.Instance.GetPlayerCharacterByClientId(clientId) == null)
            {
                PlayerSpawnManager.Instance.SpawnPlayerCharacterServerRpc(clientId, _spawnPosition, Quaternion.identity);
                _spawnPosition = new Vector3(Random.Range(0.0f, 3.0f), 0.0f, Random.Range(0.0f, 3.0f));
            }
        }

        void Update()
        {
        
        }
    }
}