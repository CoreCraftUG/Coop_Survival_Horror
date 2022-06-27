using System.Collections;
using System.Collections.Generic;
using CoreCraft.Character;
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
        public ulong PlayerObjectId;

        public NetworkingLobbyPlayerState(ulong clientId, string playerName, bool isReady, SteamId steamId)
        {
            ClientId = clientId;
            PlayerName = playerName;
            IsReady = isReady;
            SteamId = steamId;
            PlayerObjectId = 0;
        }
        
        public void SetPlayerController(ulong playerId)
        {
            PlayerObjectId = playerId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref SteamId);
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref PlayerObjectId);
        }
    }
}