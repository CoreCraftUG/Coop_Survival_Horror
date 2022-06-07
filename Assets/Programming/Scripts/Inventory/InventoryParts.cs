using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.Inventory
{
    [CreateAssetMenu(fileName = "NewPartsInventory", menuName = "InventorySystem/Inventories/PartsInventory")]
    public class InventoryParts : InventoryBase
    {
        

        public override void AddItem(ItemsBase item, int amount)
        {
            base.AddItem(item, amount);

            if (ItemList.Count >= _slotCount)
                return;

            for(int i = 0; i < ItemList.Count; i++)
            {
                if (ItemList[i].Item != item)
                    continue;

                else
                {
                    ItemList[i].Amount += amount;
                    return;
                }
            }
            ItemList.Add(new InventorySlot(item, amount));
        }

        /*
        public override void RemoveItem(ItemsBase item, int amount)
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
