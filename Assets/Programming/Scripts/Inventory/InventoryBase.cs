using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CoreCraft.Inventory
{
    public class InventoryBase : ScriptableObject
    {
        public EItemType InventoryType;
        [SerializeField]
        public int SlotCount;
        public List<InventorySlot> ItemServerList = new List<InventorySlot>();
        public List<InventorySlot> ItemClientList = new List<InventorySlot>();

        #region SyncItemList

        [ServerRpc]
        public void SyncItemListServerRpc()
        {
            SyncItemListClientRpc(ItemServerList.ToArray());
        }

        [ClientRpc]
        protected void SyncItemListClientRpc(InventorySlot[] inventorySlots)
        {
            ItemClientList = inventorySlots.ToList();
        }

        #endregion

        public virtual void Awake()
        {
            ItemServerList.Clear();
            SyncItemListServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public virtual void AddItemServerRpc(ItemsBase item, int amount)
        {
            if (item.ItemType != InventoryType)
                return;

            Debug.Log($"Client: {NetworkManager.Singleton.LocalClientId} Added Item: {item} Amount: {amount}");
        }

        [ServerRpc]
        public virtual void RemoveItemServerRpc(ItemsBase item, int amount)
        {

        }
    }

    
    [System.Serializable]
    public class InventorySlot
    {

        public ItemsBase Item;
        public int Amount;
        public InventorySlot(ItemsBase item, int amount)
        {
            Item = item;
            Amount = amount;
        }
    }
}
