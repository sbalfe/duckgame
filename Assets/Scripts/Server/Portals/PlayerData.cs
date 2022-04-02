namespace Server.Portals
{

    /* We use this class to send over the handshake to the server */
    public struct PlayerData
    {
        public string PlayerName { get; private set; }
        public ulong ClientId { get; private set; }

        public PlayerData(string playerName, ulong clientId)
        {
            PlayerName = playerName;
            ClientId = clientId;
        }
    }
}