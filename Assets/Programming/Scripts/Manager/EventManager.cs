using System.Collections;
using System.Collections.Generic;
using CoreCraft.Character;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Logger = CoreCraft.Core.Logger;


namespace CoreCraft
{
    public class EventManager : NetworkBehaviour
    {
        public static EventManager Instance { get; private set; }

        public UnityEvent EnemySetFree = new UnityEvent();
        public UnityEvent<bool> GameStarted = new UnityEvent<bool>();
        public UnityEvent<ulong> InventoryReady = new UnityEvent<ulong>();

        public UnityEvent<ulong, ulong> PlayerSpawned =
            new UnityEvent<ulong, ulong>();

        void Awake()
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

            DontDestroyOnLoad(this);
        }
    }
}