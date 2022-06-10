using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.Inventory
{
    [CreateAssetMenu(fileName = "New Base Item", menuName = "InventorySystem/Items/BaseToolItem")]
    public class ItemsToolBase : ItemsBase
    {
        public virtual void ToolFunction()
        {

        }
    }
}
