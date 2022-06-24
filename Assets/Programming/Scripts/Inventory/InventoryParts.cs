using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CoreCraft.Inventory
{
    [CreateAssetMenu(fileName = "NewPartsInventory", menuName = "InventorySystem/Inventories/PartsInventory")]
    public class InventoryParts : InventoryBase
    {
        [ServerRpc(RequireOwnership = false)]
        public override void AddItemServerRpc(ItemsBase item, int amount)
        {
            base.AddItemServerRpc(item, amount);
            if (ItemServerList.Count >= SlotCount)
                return;
            for(int i = 0; i < ItemServerList.Count; i++)
            {
                if (ItemServerList[i].Item != item)
                    continue;
                else
                {
                    ItemServerList[i].Amount += amount;
                    return;
                }
            }
            ItemServerList.Add(new InventorySlot(item, amount));
            SyncItemListServerRpc();
        }

        /*
        public override void RemoveItemServerRpc(ItemsBase item, int amount)
        {
            if (!PartInventory.Contains(item))
                return;

            for(int i = PartInventory.Count-1; i >= 0; i--)
            {
                if (PartInventory[i] != item)
                    continue;
                else
                {
                    if (amount > (PartInventory[i] as ItemsPartBase).PartCount)
                        return;
                    if (amount == (PartInventory[i] as ItemsPartBase).PartCount)
                        PartInventory.Remove(item);
                    if (amount < (PartInventory[i] as ItemsPartBase).PartCount)
                        (PartInventory[i] as ItemsPartBase).PartCount -= amount;

                    return;
                }
            }
      
        }
          */
    }
}
