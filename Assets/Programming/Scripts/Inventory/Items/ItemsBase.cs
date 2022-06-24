using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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

        // public ulong NetworkId;
        // private NetworkObject _networkObject;
        //
        // public ItemsBase()
        // {
        //     _networkObject = this.AddComponent<NetworkObject>();
        //     NetworkId = _networkObject.NetworkObjectId;
        // }
    }
}
