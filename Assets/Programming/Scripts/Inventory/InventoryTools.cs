using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CoreCraft.Inventory
{
    [CreateAssetMenu(fileName = "NewPartsInventory", menuName = "InventorySystem/Inventories/ToolsInventory")]
    public class InventoryTools : InventoryBase
    {
        [ServerRpc(RequireOwnership = false)]
        public override void AddItemServerRpc(ItemsBase item, int amount)
        {
            base.AddItemServerRpc(item, amount);

            if (ItemServerList.Count >= SlotCount)
                return;

            ItemServerList.Add(new InventorySlot(item, 1));
            SyncItemListServerRpc();
        }
        /*
        public override void RemoveItemServerRpc(ItemsBase item, int amount)
        {
            base.RemoveItemServerRpc(item, amount);

            if (ToolInventory.Contains(item))
                ToolInventory.Remove(item);
        }
        */
    }

   
}
