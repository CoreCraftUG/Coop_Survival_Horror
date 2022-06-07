using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.Inventory
{
    [CreateAssetMenu(fileName ="New Base Item", menuName = "InventorySystem/Items/BaseItem")]
    public class ItemsPartBase : ItemsBase
    {
        public int PartCount;
    }
}
