using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.Inventory
{
    public class InventoryTools : InventoryBase
    {
        public List<ItemsBase> ToolInventory;
        private void Awake()
        {
            InventoryType = EItemType.Tool;
        }

        public override void AddItem(ItemsBase item, int amount)
        {
            base.AddItem(item, amount);

            if (ToolInventory.Count >= _slotCount)
                return;

            ToolInventory.Add(item);
        }

        public override void RemoveItem(ItemsBase item, int amount)
        {
            base.RemoveItem(item, amount);

            if (ToolInventory.Contains(item))
                ToolInventory.Remove(item);
        }
    }

   
}
