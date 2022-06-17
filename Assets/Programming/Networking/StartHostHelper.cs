using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NM = Unity.Netcode.NetworkManager;

namespace CoreCraft.Programming.HelpersAndTools
{
    public class StartHostHelper : MonoBehaviour
    {
        void Start()
        {
            NM.Singleton.StartHost();
        }
    }
}