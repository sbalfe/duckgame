using System;
using Unity.Collections;
using Unity.Netcode;

namespace Client.UI
{

    /* Lobby player state , is network serializable and IEquatable meaning you can compare */
    public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
    {
        /* stores these attributes on the state */
        public ulong ClientId;
        public FixedString32Bytes PlayerName;
        public bool IsReady;

        /* constructor to set values*/
        public LobbyPlayerState(ulong clientId, FixedString32Bytes playerName, bool isReady)
        {
            ClientId = clientId;
            PlayerName = playerName;
            IsReady = isReady;
        }

        /* serialize each of the values ready for the NetCode*/
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref IsReady);
        }

        /* Must implement the equals method from */
        public bool Equals(LobbyPlayerState other)
        {
            /* overloaded equals operator  */
            return ClientId == other.ClientId &&
                PlayerName.Equals(other.PlayerName) &&
                IsReady == other.IsReady;
        }
    }
}
