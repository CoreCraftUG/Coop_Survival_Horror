using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

namespace CoreCraft.Networking
{
    public struct NetworkingLobbyPlayerState : INetworkSerializable
    {
        public ulong SteamId;
        public ulong ClientId;
        public string PlayerName;
        public bool IsReady;

        public NetworkingLobbyPlayerState(ulong clientId, string playerName, bool isReady, SteamId steamId)
        {
            ClientId = clientId;
            PlayerName = playerName;
            IsReady = isReady;
            SteamId = steamId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref SteamId);
        }
    }
}