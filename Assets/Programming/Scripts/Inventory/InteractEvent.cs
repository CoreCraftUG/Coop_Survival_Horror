using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CoreCraft
{
    public class InteractEvent : MonoBehaviour
    {
        public UnityEvent<ulong> Interacted = new UnityEvent<ulong>();

        void Start()
        {
            transform.tag = "Interactable";
        }

        void Update()
        {
        
        }

        public void Interact(ulong clientId)
        {
            Debug.Log($"Client: {clientId} Interacted with {gameObject}");
            Interacted.Invoke(clientId);
        }
    }
}