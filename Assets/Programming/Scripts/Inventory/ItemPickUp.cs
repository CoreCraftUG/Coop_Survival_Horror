using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreCraft.Inventory;
using CoreCraft.Networking;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using CoreCraft.Minigames;
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
        private Vector3 _playerCameraPos;
        private Vector3 _playerCameraRot;

        private NetworkVariable<ulong> _ownedMiniGameNetworkId = new NetworkVariable<ulong>();

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
            if (Physics.Raycast(_playerController.PlayerCamera.transform.position, _playerController.PlayerCamera.transform.forward, out RaycastHit hit, _itemRange))
            {
                RaycastHit oldHit = _itemHit;
                _itemHit = hit;
                if (_itemHit.transform.tag == "Item")
                {

                    if (_itemHit.transform != null && _itemHit.transform != hit.transform)
                        if (_itemHit.transform.GetComponentInChildren<ItemOutline>() != null)
                            if (_itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled)
                                oldHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled = false;
                    

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
                if (_itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled)
                    _itemHit.transform.GetComponentInChildren<ItemOutline>().OutlineRenderer.enabled = false;
            Debug.DrawRay(_playerController.PlayerCamera.transform.position, _playerController.PlayerCamera.transform.forward * _itemRange, Color.green, 0.1f);
        }

        public void LeaveMinigame(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                ResetOwnerServerRpc();
                this.transform.GetComponent<PlayerController>().MiniGameInteraction(true, Vector3.zero);
                this.transform.GetComponent<PlayerInput>().SwitchCurrentActionMap("PlayerMap");
            }
        }

        public void Interact(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                if (_itemHit.collider.transform.tag == "Interactable")
                {
                    _itemHit.collider.transform.GetComponent<InteractEvent>().Interact(NetworkManager.Singleton.LocalClientId);
                }
                else if (_itemHit.transform.tag == "Item" && _itemHit.transform != null && _itemHit.transform.GetComponentInChildren<ItemOutline>() != null)
                {
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
                else if (_itemHit.transform.tag == "Minigame")
                {
                    SpawnAsOwnerServerRpc(_itemHit.transform.GetComponent<NetworkObject>().NetworkObjectId, NetworkManager.Singleton.LocalClientId);
                    SwitchInputActionMapServerRpc(NetworkManager.Singleton.LocalClientId, "MinigameMap");
                    this.transform.GetComponent<PlayerController>().MiniGameInteraction(true, _itemHit.transform.GetComponent<BaseMinigame>().MinigamePos);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SwitchInputActionMapServerRpc(ulong clientId, string actionMapName)
        {
            SwitchInputActionMapClientRpc(clientId,actionMapName);
        }

        [ClientRpc]
        private void SwitchInputActionMapClientRpc(ulong clientId, string actionMapName)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                this.transform.GetComponent<PlayerInput>().SwitchCurrentActionMap(actionMapName);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnAsOwnerServerRpc(ulong hitId, ulong clientId)
        {
            ResetOwnerServerRpc();
            NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[hitId];
            networkObject.ChangeOwnership(clientId);
            _ownedMiniGameNetworkId.Value = hitId;
        }

        [ServerRpc(RequireOwnership = false)]
        private void ResetOwnerServerRpc()
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(_ownedMiniGameNetworkId.Value))
                return;

            NetworkObject nObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_ownedMiniGameNetworkId.Value];

            if (nObject.IsOwnedByServer)
                return;

            nObject.RemoveOwnership();
            _ownedMiniGameNetworkId = new NetworkVariable<ulong>();
        }

        [ServerRpc(RequireOwnership = false)]
        private void InteractServerRpc(ulong networkId)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkId].Despawn();
        }
    }
}
