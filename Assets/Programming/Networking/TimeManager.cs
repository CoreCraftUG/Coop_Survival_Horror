using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace CoreCraft
{
    public class TimeManager : NetworkBehaviour
    {
        private bool _gameStarted;
        private NetworkObject thisNetworkObject => GetComponent<NetworkObject>();
        private float _timer;

        [SerializeField] private List<TimeBasedEventStruct> EventList = new List<TimeBasedEventStruct>();

        void Start()
        {
            EventManager.Instance.GameStarted.AddListener((started => { thisNetworkObject.Spawn(); _gameStarted = started;}));
        }
        
        void FixedUpdate()
        {
            if (!_gameStarted || !IsServer)
                return;


            _timer += NetworkManager.Singleton.ServerTime.FixedDeltaTime;
        }
    }

    public struct TimeBasedEventStruct
    {
        public Vector2 TimeInterval;

        public float TriggerChance;

        public UnityEvent TimeBasedEvent;
    }
}
