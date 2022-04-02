using System;

namespace Server.Portals
{

    /* Data we send over the client instance to connect to the server */
    [Serializable]
    public class ConnectionPayload
    {
        public string clientGUID;
        public int clientScene = -1;
        public string playerName;
    }
}