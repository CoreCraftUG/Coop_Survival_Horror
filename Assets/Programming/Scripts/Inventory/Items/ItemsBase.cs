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
    public class ItemsBase : ScriptableObject
    {
        private GameObject _visual;
        public EItemType ItemType;
        [TextArea(15, 20)]
        public string Description;


    }
}
