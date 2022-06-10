using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.Inventory
{
    public enum EItemType
    {
        Tool,
        Part
    }
    [CreateAssetMenu(fileName = "New Base Item", menuName = "InventorySystem/Items/DefaultItem")]
    public class ItemsBase : ScriptableObject
    {
        public GameObject Visual;
        public EItemType ItemType;
        [TextArea(15, 20)]
        public string Description;


    }
}
