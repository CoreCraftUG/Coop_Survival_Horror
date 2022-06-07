using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreCraft.Inventory;

namespace CoreCraft
{
    public class JulianTestPlayer : MonoBehaviour
    {
        public InventoryTools ToolInventory;
        public InventoryParts PartsInventory;

        public void OnTriggerEnter(Collider other)
        {
            var item = other.GetComponent<ItemsBase>();
            if (item)
            {

                if (item.ItemType == EItemType.Part)
                {
                    PartsInventory.AddItem(item, 1);
                    Destroy(other.gameObject);
                }
            }
        }
    }
}
