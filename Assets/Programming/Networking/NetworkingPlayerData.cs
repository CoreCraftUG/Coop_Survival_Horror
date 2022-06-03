namespace CoreCraft.Networking
{
    public struct PlayerData
    {
        public string PlayerName { get; private set; }
        public ulong ClientId { get; private set; }
        public ulong SteamId { get; private set; }

        public PlayerData(string playerName, ulong clientId, ulong steamId)
        {
            PlayerName = playerName;
            ClientId = clientId;
            SteamId = steamId;
        }
    }
}