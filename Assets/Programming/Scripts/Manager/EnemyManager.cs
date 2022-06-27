using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreCraft.Character;
using CoreCraft.Core;
using CoreCraft.Networking;
using Unity.Netcode;
using UnityEngine;
using Logger = CoreCraft.Core.Logger;
using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft.Enemy
{
    public class EnemyManager : NetworkBehaviour
    {
        public static EnemyManager Instance;

        [SerializeField] private GameObject _enemySpawnObject;

        private bool _enemySpawned;
        private GameObject _enemyObject;
        private Enemy _enemy;

        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private List<NetworkingLobbyPlayerState> _playerStates;

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

            DontDestroyOnLoad(this);
        }

        void Start()
        {
            EventManager.Instance.GameStarted.AddListener(StartGame);

            NM.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NM.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        private void OnDestroy()
        {
            NM.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NM.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            _playerStates = GameManager.Instance.GetPlayerInfos();
        }

        private void HandleClientConnected(ulong clientId)
        {
            _playerStates = GameManager.Instance.GetPlayerInfos();
        }

        private void StartGame(bool gameStarted)
        {
            if (gameStarted)
            {
                _playerStates = GameManager.Instance.GetPlayerInfos();
                if (_enemySpawned)
                {
                    _enemyObject.SetActive(true);
                }
                else
                {
                    _enemyObject = Instantiate(_enemySpawnObject, _spawnPosition, _spawnRotation);
                    _enemy = _enemyObject.GetComponent<Enemy>();
                }
                EventManager.Instance.EnemySetFree.Invoke();
            }
            else if (_enemySpawned)
            {
                _enemyObject.SetActive(false);
            }
        }

        void Update()
        {

        }

        public void TriggerSound(Vector3 position)
        {

        }

        public PlayerController GetNearestPlayer()
        {
            float tempDistance = 0f;
            PlayerController targetPlayer = null;
            foreach (NetworkingLobbyPlayerState state in _playerStates.Where(s => NM.Singleton.SpawnManager.SpawnedObjects[s.PlayerObjectId] != null && NM.Singleton.SpawnManager.SpawnedObjects[s.PlayerObjectId].GetComponent<PlayerController>().IsAlive))
            {
                float d = Vector3.Distance(_enemyObject.transform.position,
                    NM.Singleton.SpawnManager.SpawnedObjects[state.PlayerObjectId].GetComponent<PlayerController>().PhysicsCharacter.transform.position);

                if (d < tempDistance || targetPlayer == null)
                {
                    targetPlayer = NM.Singleton.SpawnManager.SpawnedObjects[state.PlayerObjectId].GetComponent<PlayerController>();
                    tempDistance = d;
                }
            }

            return targetPlayer;
        }

        public void SetEnemySpawnPosition(Vector3 position)
        {
            _spawnPosition = position;
        }

        public void SetEnemySpawnRotation(Quaternion rotation)
        {
            _spawnRotation = rotation;
        }
    }
}
