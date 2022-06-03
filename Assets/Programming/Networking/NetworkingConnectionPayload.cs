using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreCraft.Networking
{
    [Serializable]
    public class NetworkingConnectionPayload
    {
        public string Password;
        public string PlayerName;
        public string ClientGUID;
        public int ClientScene = -1;
        public ulong SteamId;
    }
}