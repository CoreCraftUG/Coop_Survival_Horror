using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CoreCraft
{
    public class StickScript : NetworkBehaviour
    {
        public List<GameObject> BlockList;
        public List<GameObject> Slots;
    }
}
