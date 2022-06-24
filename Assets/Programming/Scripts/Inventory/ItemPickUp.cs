using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreCraft.Inventory;
using CoreCraft.Networking;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CoreCraft.Character
{
    public class ItemPickUp : NetworkBehaviour
    {
        [SerializeField] private InventoryTools _toolInventoryPrefab;
        [SerializeField] private InventoryParts _partInventoryPrefab;

        [SerializeField] private float _itemRange;

        // [HideInInspector] 
        public InventoryTools ToolInventory;
        // [HideInInspector]
        public InventoryParts PartsInventory;


        private RaycastHit _itemHit;
        public float MaxOxygen;
        public float CurrentOxygen;
        private ItemOutline itemOutline;
        private PlayerController _playerController;

        public void Start()
        {
            _playerController = GetComponent<PlayerController>();

            PrepareInventoryClientRpc();
        }

        [ClientRpc]
        private void PrepareInventoryClientRpc()
        {
            if (!IsOwner)
                return;

            ToolInventory = new InventoryTools { InventoryType = EItemType.Tool, SlotCount = 5 };
            ToolInventory.name = $"Client: {NetworkManager.Singleton.LocalClientId} Tool Inventory";

            PartsInventory = new InventoryParts { InventoryType = EItemType.Part, SlotCount = 5 };
            PartsInventory.name = $"Client: {NetworkManager.Singleton.LocalClientId} Part Inventory";

            EventManager.Instance.InventoryReady.Invoke(NetworkManager.Singleton.LocalClientId);
        }

        public void FixedUpdate()
        {
            if (Physics.Raycast(_playerController.PlayerCamera.transform.position, _playerController.PlayerCamera.transform.forward, out RaycastHit hitInfo, _itemRange))
            {
                if (hitInfo.transform.tag == "Item")
                {

                    if (_itemHit.transform != null && _itemHit.transform != hitInfo.transform)
                        if (_itemHit.transform.GetComponentInChildren<ItemOutline>() != null)
                            if (_itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled == true)
                                _itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled = false;

                    _itemHit = hitInfo;

                    if (_itemHit.transform.GetComponentInChildren<ItemOutline>())
                        _itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled = true;
                }
                else
                {
                    if (_itemHit.transform != null && _itemHit.transform.GetComponentInChildren<ItemOutline>() != null)
                        _itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled = false;
                }
            }
            else if (_itemHit.transform != null && _itemHit.transform.GetComponentInChildren<ItemOutline>() != null)
                if (_itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled == true)
                    _itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled = false;
            Debug.DrawRay(_playerController.PlayerCamera.transform.position, _playerController.PlayerCamera.transform.forward * _itemRange, Color.green, 0.1f);
        }

        public void Interact(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                if (_itemHit.transform != null && _itemHit.transform.GetComponentInChildren<ItemOutline>() != null)
                    if (_itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled)
                    {
                        var item = _itemHit.transform.GetComponent<TestItem>();
                        if (item != null)
                        {
                            if (item.Item.ItemType == EItemType.Part)
                            {
                                PartsInventory.AddItemServerRpc(item.Item, (item.Item as ItemsPartBase).PartCount);
                                InteractServerRpc(_itemHit.transform.GetComponent<NetworkObject>().NetworkObjectId);
                            }

                            if (item.Item.ItemType == EItemType.Tool)
                            {
                                ToolInventory.AddItemServerRpc(item.Item, 1);
                                InteractServerRpc(_itemHit.transform.GetComponent<NetworkObject>().NetworkObjectId);
                            }
                        }
                    }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void InteractServerRpc(ulong networkId)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkId].Despawn();
        }
    }
}
