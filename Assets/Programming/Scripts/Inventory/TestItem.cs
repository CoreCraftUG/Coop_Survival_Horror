using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreCraft.Inventory;
using Unity.Netcode;

namespace CoreCraft
{
    public class TestItem : NetworkBehaviour
    {
        public ItemsBase Item;
    }
}
