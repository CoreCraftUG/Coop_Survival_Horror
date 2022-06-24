using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Logger = CoreCraft.Core.Logger;


namespace CoreCraft
{
    public class EventManager : NetworkBehaviour
    {
        public static EventManager Instance { get; private set; }


        public UnityEvent<ulong> InventoryReady = new UnityEvent<ulong>();

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