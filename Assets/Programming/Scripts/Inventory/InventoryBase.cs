using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.Inventory
{
    public class InventoryBase : ScriptableObject
    {
        public EItemType InventoryType;
        [SerializeField]
        protected int _slotCount;
        public List<InventorySlot> ItemList = new List<InventorySlot>();

        public virtual void Awake()
        {
            ItemList.Clear();
        }
        public virtual void AddItem(ItemsBase item, int amount)
        {
            if (item.ItemType != InventoryType)
                return;               
        }

        public virtual void RemoveItem(ItemsBase item, int amount)
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
