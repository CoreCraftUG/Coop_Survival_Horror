using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.Inventory
{
    [CreateAssetMenu(fileName = "NewPartsInventory", menuName = "InventorySystem/Inventories/ToolsInventory")]
    public class InventoryTools : InventoryBase
    {


        public override void Awake()
        {
            base.Awake();
            ItemList.Clear();
        }

        

        public override void AddItem(ItemsBase item, int amount)
        {
            base.AddItem(item, amount);

            if (ItemList.Count >= _slotCount)
                return;

            ItemList.Add(new InventorySlot(item, 1));
        }
        /*
        public override void RemoveItem(ItemsBase item, int amount)
        {
            base.RemoveItem(item, amount);

            if (ToolInventory.Contains(item))
                ToolInventory.Remove(item);
        }
        */
    }

   
}
